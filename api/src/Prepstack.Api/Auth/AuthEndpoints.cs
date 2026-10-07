using FluentValidation;
using Microsoft.Extensions.Options;
using Prepstack.Api.Common;
using Prepstack.Application.Auth;
using Prepstack.Application.Auth.Commands;
using Prepstack.Domain.Common;

namespace Prepstack.Api.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").RequireRateLimiting(RateLimiting.AuthPolicy);

        group.MapPost("/register", RegisterAsync)
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", LoginAsync)
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", RefreshAsync)
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", LogoutAsync)
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterCommand command,
        IValidator<RegisterCommand> validator,
        RegisterCommandHandler handler,
        HttpContext httpContext,
        IOptions<AuthCookieOptions> cookieOptions,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToValidationProblem();
        }

        var result = await handler.Handle(command, cancellationToken);
        return ToHttpResult(result, httpContext, cookieOptions.Value, environment);
    }

    private static async Task<IResult> LoginAsync(
        LoginCommand command,
        IValidator<LoginCommand> validator,
        LoginCommandHandler handler,
        HttpContext httpContext,
        IOptions<AuthCookieOptions> cookieOptions,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToValidationProblem();
        }

        var result = await handler.Handle(command, cancellationToken);
        return ToHttpResult(result, httpContext, cookieOptions.Value, environment);
    }

    private static async Task<IResult> RefreshAsync(
        HttpContext httpContext,
        IValidator<RefreshCommand> validator,
        RefreshCommandHandler handler,
        IOptions<AuthCookieOptions> cookieOptions,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        var rawToken = httpContext.Request.Cookies[cookieOptions.Value.Name] ?? string.Empty;
        var command = new RefreshCommand(rawToken);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            RefreshCookieWriter.Clear(httpContext, cookieOptions.Value, environment);
            return Error.Unauthorized("auth.invalid_refresh_token", "Refresh token is required.").ToProblem();
        }

        var result = await handler.Handle(command, cancellationToken);
        if (!result.IsSuccess)
        {
            RefreshCookieWriter.Clear(httpContext, cookieOptions.Value, environment);
            return result.Error!.ToProblem();
        }

        RefreshCookieWriter.Append(httpContext, cookieOptions.Value, environment, result.Value);
        return Results.Ok(new AuthResponse(result.Value.AccessToken, result.Value.AccessTokenExpiresAt));
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        LogoutCommandHandler handler,
        IOptions<AuthCookieOptions> cookieOptions,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        var rawToken = httpContext.Request.Cookies[cookieOptions.Value.Name];
        await handler.Handle(new LogoutCommand(rawToken), cancellationToken);
        RefreshCookieWriter.Clear(httpContext, cookieOptions.Value, environment);
        return Results.NoContent();
    }

    private static IResult ToHttpResult(
        Result<AuthTokens> result,
        HttpContext httpContext,
        AuthCookieOptions cookieOptions,
        IWebHostEnvironment environment)
    {
        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem();
        }

        RefreshCookieWriter.Append(httpContext, cookieOptions, environment, result.Value);
        return Results.Ok(new AuthResponse(result.Value.AccessToken, result.Value.AccessTokenExpiresAt));
    }
}
