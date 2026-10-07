using Prepstack.Domain.Common;

namespace Prepstack.Application.Auth.Commands;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    TimeProvider timeProvider)
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.RawToken))
        {
            return Result.Success();
        }

        var tokenHash = refreshTokenGenerator.Hash(command.RawToken);
        var existing = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (existing is null || existing.RevokedAt is not null)
        {
            return Result.Success();
        }

        existing.Revoke(timeProvider.GetUtcNow());
        await refreshTokenRepository.RevokeAsync(existing, cancellationToken);
        return Result.Success();
    }
}
