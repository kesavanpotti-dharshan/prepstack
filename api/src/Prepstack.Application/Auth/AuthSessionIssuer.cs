using Prepstack.Application.Common;
using Prepstack.Domain.Auth;

namespace Prepstack.Application.Auth;

/// <summary>Issues a brand-new session (access token + first refresh token of a new family).</summary>
public sealed class AuthSessionIssuer(
    IAccessTokenGenerator accessTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IIdGenerator idGenerator)
{
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

    public async Task<AuthTokens> IssueNewSessionAsync(User user, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var accessToken = accessTokenGenerator.Generate(user, now);
        var secret = refreshTokenGenerator.Generate();
        var familyId = idGenerator.NewId();

        var refreshToken = RefreshToken.Issue(idGenerator.NewId(), user.Id, secret.Hash, familyId, now, RefreshTokenLifetime);
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return new AuthTokens(accessToken.Value, accessToken.ExpiresAt, secret.RawToken, refreshToken.ExpiresAt);
    }
}
