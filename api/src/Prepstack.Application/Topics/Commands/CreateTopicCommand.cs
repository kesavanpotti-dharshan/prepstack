namespace Prepstack.Application.Topics.Commands;

public sealed record CreateTopicCommand(
    string OwnerId,
    string Name,
    string? ParentId,
    string? Description,
    string Visibility);
