namespace Prepstack.Api.Auth;

public sealed class AuthCookieOptions
{
    public const string SectionName = "Auth:Cookie";

    public string Name { get; init; } = "prepstack_rt";

    /// <summary>Null in local dev; set to ".&lt;domain&gt;" once app/api share a registrable domain (architecture.md §9).</summary>
    public string? Domain { get; init; }
}
