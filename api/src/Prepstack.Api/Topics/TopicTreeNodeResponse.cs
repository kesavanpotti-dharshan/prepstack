namespace Prepstack.Api.Topics;

public sealed record TopicTreeNodeResponse(
    string Id,
    string Slug,
    string Name,
    string? ParentId,
    string Path,
    string? Description,
    string Visibility,
    int QuestionCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<TopicTreeNodeResponse> Children);
