namespace Prepstack.Application.Topics.Commands;

public sealed record UpdateTopicCommand(
    string OwnerId,
    string TopicId,
    string Name,
    string? Description,
    string Visibility);
