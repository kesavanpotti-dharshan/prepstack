namespace Prepstack.Domain.Auth;

/// <summary>
/// One link in a rotation chain (<see cref="FamilyId"/>). Reusing a token that has
/// already been rotated out is a sign of theft — callers revoke the whole family
/// when that happens, not just this token (rules.md → MongoDB / Security).
/// </summary>
public sealed class RefreshToken
{
    public string Id { get; }

    public string UserId { get; }

    public string TokenHash { get; }

    public string FamilyId { get; }

    public DateTimeOffset ExpiresAt { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public string? ReplacedBy { get; private set; }

    private RefreshToken(
        string id,
        string userId,
        string tokenHash,
        string familyId,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt,
        DateTimeOffset? revokedAt,
        string? replacedBy)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        FamilyId = familyId;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        RevokedAt = revokedAt;
        ReplacedBy = replacedBy;
    }

    public static RefreshToken Issue(
        string id,
        string userId,
        string tokenHash,
        string familyId,
        DateTimeOffset now,
        TimeSpan lifetime)
        => new(id, userId, tokenHash, familyId, now + lifetime, now, revokedAt: null, replacedBy: null);

    public static RefreshToken Reconstitute(
        string id,
        string userId,
        string tokenHash,
        string familyId,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt,
        DateTimeOffset? revokedAt,
        string? replacedBy)
        => new(id, userId, tokenHash, familyId, expiresAt, createdAt, revokedAt, replacedBy);

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now, string? replacedBy = null)
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = now;
        ReplacedBy = replacedBy;
    }
}
