using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Prepstack.Domain.Auth;

namespace Prepstack.Infrastructure.Auth;

internal sealed class RefreshTokenDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; init; }

    public required string UserId { get; init; }

    public required string TokenHash { get; init; }

    public required string FamilyId { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? RevokedAt { get; set; }

    public string? ReplacedBy { get; set; }

    public static RefreshTokenDocument FromDomain(RefreshToken token) => new()
    {
        Id = token.Id,
        UserId = token.UserId,
        TokenHash = token.TokenHash,
        FamilyId = token.FamilyId,
        ExpiresAt = token.ExpiresAt,
        CreatedAt = token.CreatedAt,
        RevokedAt = token.RevokedAt,
        ReplacedBy = token.ReplacedBy,
    };

    public RefreshToken ToDomain() =>
        RefreshToken.Reconstitute(Id, UserId, TokenHash, FamilyId, ExpiresAt, CreatedAt, RevokedAt, ReplacedBy);
}
