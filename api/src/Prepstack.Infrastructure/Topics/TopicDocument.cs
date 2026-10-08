using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Prepstack.Domain.Topics;

namespace Prepstack.Infrastructure.Topics;

internal sealed class TopicDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; init; }

    public required string OwnerId { get; init; }

    public required string Slug { get; init; }

    public required string Name { get; init; }

    public string? ParentId { get; init; }

    public required string Path { get; init; }

    public string? Description { get; init; }

    public required string Visibility { get; init; }

    public required int QuestionCount { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public static TopicDocument FromDomain(Topic topic) => new()
    {
        Id = topic.Id,
        OwnerId = topic.OwnerId,
        Slug = topic.Slug,
        Name = topic.Name,
        ParentId = topic.ParentId,
        Path = topic.Path,
        Description = topic.Description,
        Visibility = topic.Visibility,
        QuestionCount = topic.QuestionCount,
        CreatedAt = topic.CreatedAt,
        UpdatedAt = topic.UpdatedAt,
    };

    public Topic ToDomain() => Topic.Reconstitute(
        Id,
        OwnerId,
        Slug,
        Name,
        ParentId,
        Path,
        Description,
        Visibility,
        QuestionCount,
        CreatedAt,
        UpdatedAt);
}
