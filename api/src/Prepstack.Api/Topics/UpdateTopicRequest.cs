namespace Prepstack.Api.Topics;

public sealed record UpdateTopicRequest(string Name, string? Description, string Visibility);
