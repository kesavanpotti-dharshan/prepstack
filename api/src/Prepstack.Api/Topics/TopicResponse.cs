namespace Prepstack.Api.Topics;

public sealed record TopicResponse(
    string Id,
    string Slug,
    string Name,
    string? ParentId,
    string Path,
    string? Description,
    string Visibility,
    int QuestionCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
