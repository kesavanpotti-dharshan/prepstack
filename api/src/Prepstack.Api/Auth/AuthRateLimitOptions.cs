namespace Prepstack.Api.Auth;

public sealed class AuthRateLimitOptions
{
    public const string SectionName = "Auth:RateLimit";

    public int PermitLimit { get; init; } = 10;

    public int WindowSeconds { get; init; } = 60;
}
