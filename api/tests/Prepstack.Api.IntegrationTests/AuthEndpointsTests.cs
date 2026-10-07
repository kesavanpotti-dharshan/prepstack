using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Prepstack.Api.IntegrationTests;

public sealed class AuthEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string CookieName = "prepstack_rt";

    [Fact]
    public async Task Register_NewEmail_ReturnsAccessTokenAndSetsRefreshCookie()
    {
        var client = CreateClient();

        var response = await RegisterAsync(client, UniqueEmail());

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.AccessToken.ShouldNotBeNullOrWhiteSpace();
        ExtractCookieValue(response, CookieName).ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        await RegisterAsync(client, email);

        var response = await RegisterAsync(client, email);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsValidationProblem()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = UniqueEmail(), password = "short", displayName = "Test User" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_CorrectCredentials_ReturnsAccessTokenAndSetsRefreshCookie()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        await RegisterAsync(client, email);

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "password123" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        ExtractCookieValue(response, CookieName).ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var client = CreateClient();
        var email = UniqueEmail();
        await RegisterAsync(client, email);

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "wrong-password" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = UniqueEmail(), password = "password123" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ValidToken_RotatesAndInvalidatesOldToken()
    {
        var client = CreateClient();
        var registerResponse = await RegisterAsync(client, UniqueEmail());
        var originalToken = ExtractCookieValue(registerResponse, CookieName);

        var refreshResponse = await PostWithCookieAsync(client, "/api/v1/auth/refresh", originalToken);
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var rotatedToken = ExtractCookieValue(refreshResponse, CookieName);
        rotatedToken.ShouldNotBe(originalToken);

        // Replaying the rotated-out token is treated as theft...
        var reuseResponse = await PostWithCookieAsync(client, "/api/v1/auth/refresh", originalToken);
        reuseResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // ...which must revoke the WHOLE family, including the token the rotation above just issued.
        var followUpResponse = await PostWithCookieAsync(client, "/api/v1/auth/refresh", rotatedToken);
        followUpResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_MissingCookie_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/api/v1/auth/refresh", content: null);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ThenRefresh_TokenNoLongerWorks()
    {
        var client = CreateClient();
        var registerResponse = await RegisterAsync(client, UniqueEmail());
        var token = ExtractCookieValue(registerResponse, CookieName);

        var logoutResponse = await PostWithCookieAsync(client, "/api/v1/auth/logout", token);
        logoutResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var refreshResponse = await PostWithCookieAsync(client, "/api/v1/auth/refresh", token);
        refreshResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithoutCookie_IsIdempotent()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/api/v1/auth/logout", content: null);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    private HttpClient CreateClient() =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

    private static Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email) =>
        client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email, password = "password123", displayName = "Test User" });

    private static Task<HttpResponseMessage> PostWithCookieAsync(HttpClient client, string path, string cookieValue)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("Cookie", $"{CookieName}={cookieValue}");
        return client.SendAsync(request);
    }

    private static string ExtractCookieValue(HttpResponseMessage response, string cookieName)
    {
        var header = response.Headers.GetValues("Set-Cookie")
            .First(value => value.StartsWith($"{cookieName}=", StringComparison.Ordinal));
        return header.Split(';')[0][(cookieName.Length + 1)..];
    }

    private static string UniqueEmail() => $"{Guid.NewGuid():N}@example.com";

    private sealed record AuthResponseDto(string AccessToken, DateTimeOffset AccessTokenExpiresAt);
}
