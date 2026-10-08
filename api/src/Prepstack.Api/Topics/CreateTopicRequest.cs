namespace Prepstack.Api.Topics;

public sealed record CreateTopicRequest(string Name, string? ParentId, string? Description, string? Visibility);
