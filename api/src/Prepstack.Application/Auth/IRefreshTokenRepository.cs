using Prepstack.Domain.Auth;

namespace Prepstack.Application.Auth;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);

    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>Revokes <paramref name="revoked"/> and inserts <paramref name="replacement"/> in one transaction.</summary>
    Task RotateAsync(RefreshToken revoked, RefreshToken replacement, CancellationToken cancellationToken);

    /// <summary>Reuse detection: revokes every still-active token in the family.</summary>
    Task RevokeFamilyAsync(string familyId, DateTimeOffset now, CancellationToken cancellationToken);

    Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken);
}
