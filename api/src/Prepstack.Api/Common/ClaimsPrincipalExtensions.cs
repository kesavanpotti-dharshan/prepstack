using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Prepstack.Api.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The authenticated caller's user id, from the access token's `sub` claim
    /// (see JwtAccessTokenGenerator). Checks both claim-type spellings since inbound
    /// claim mapping behavior depends on JWT handler configuration.
    /// </summary>
    public static string GetOwnerId(this ClaimsPrincipal user) =>
        user.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Authenticated request is missing a subject claim.");
}
