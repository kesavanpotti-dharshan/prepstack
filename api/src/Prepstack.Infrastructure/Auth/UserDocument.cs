using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Prepstack.Domain.Auth;

namespace Prepstack.Infrastructure.Auth;

internal sealed class UserDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; init; }

    public required string Email { get; init; }

    public required string PasswordHash { get; init; }

    public required string DisplayName { get; init; }

    public required List<string> Roles { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public static UserDocument FromDomain(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        PasswordHash = user.PasswordHash,
        DisplayName = user.DisplayName,
        Roles = [.. user.Roles],
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
    };

    public User ToDomain() => User.Reconstitute(Id, Email, PasswordHash, DisplayName, Roles, CreatedAt, UpdatedAt);
}
