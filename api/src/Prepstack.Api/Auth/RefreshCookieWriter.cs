using Prepstack.Application.Auth;

namespace Prepstack.Api.Auth;

public static class RefreshCookieWriter
{
    public static void Append(
        HttpContext httpContext,
        AuthCookieOptions options,
        IWebHostEnvironment environment,
        AuthTokens tokens)
    {
        httpContext.Response.Cookies.Append(
            options.Name,
            tokens.RefreshToken,
            BuildOptions(options, environment, tokens.RefreshTokenExpiresAt));
    }

    public static void Clear(HttpContext httpContext, AuthCookieOptions options, IWebHostEnvironment environment)
    {
        httpContext.Response.Cookies.Delete(options.Name, BuildOptions(options, environment, DateTimeOffset.UnixEpoch));
    }

    private static CookieOptions BuildOptions(AuthCookieOptions options, IWebHostEnvironment environment, DateTimeOffset expires) =>
        new()
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/api/v1/auth",
            Domain = options.Domain,
            Expires = expires,
        };
}
