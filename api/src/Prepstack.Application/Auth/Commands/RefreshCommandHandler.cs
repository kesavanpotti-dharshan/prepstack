using Microsoft.Extensions.Logging;
using Prepstack.Application.Common;
using Prepstack.Domain.Auth;
using Prepstack.Domain.Common;

namespace Prepstack.Application.Auth.Commands;

public sealed class RefreshCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IAccessTokenGenerator accessTokenGenerator,
    IIdGenerator idGenerator,
    TimeProvider timeProvider,
    ILogger<RefreshCommandHandler> logger)
{
    public async Task<Result<AuthTokens>> Handle(RefreshCommand command, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var tokenHash = refreshTokenGenerator.Hash(command.RawToken);
        var existing = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existing is null)
        {
            return Error.Unauthorized("auth.invalid_refresh_token", "Refresh token is invalid.");
        }

        if (existing.RevokedAt is not null)
        {
            logger.LogWarning(
                "Refresh token reuse detected for family {FamilyId}; revoking the whole family",
                existing.FamilyId);
            await refreshTokenRepository.RevokeFamilyAsync(existing.FamilyId, now, cancellationToken);
            return Error.Unauthorized("auth.refresh_token_reused", "Session has been revoked. Please log in again.");
        }

        if (!existing.IsActive(now))
        {
            return Error.Unauthorized("auth.refresh_token_expired", "Refresh token has expired.");
        }

        var user = await userRepository.GetByIdAsync(existing.UserId, cancellationToken);
        if (user is null)
        {
            return Error.Unauthorized("auth.invalid_refresh_token", "Refresh token is invalid.");
        }

        var secret = refreshTokenGenerator.Generate();
        var replacement = RefreshToken.Issue(
            idGenerator.NewId(),
            user.Id,
            secret.Hash,
            existing.FamilyId,
            now,
            AuthSessionIssuer.RefreshTokenLifetime);
        existing.Revoke(now, replacedBy: replacement.Id);

        await refreshTokenRepository.RotateAsync(existing, replacement, cancellationToken);

        var accessToken = accessTokenGenerator.Generate(user, now);
        return Result<AuthTokens>.Success(
            new AuthTokens(accessToken.Value, accessToken.ExpiresAt, secret.RawToken, replacement.ExpiresAt));
    }
}
