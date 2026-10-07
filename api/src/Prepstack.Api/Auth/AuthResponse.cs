namespace Prepstack.Api.Auth;

public sealed record AuthResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAt);
