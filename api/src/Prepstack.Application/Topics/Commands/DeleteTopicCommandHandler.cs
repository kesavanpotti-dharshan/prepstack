using Prepstack.Domain.Common;

namespace Prepstack.Application.Topics.Commands;

public sealed class DeleteTopicCommandHandler(ITopicRepository topicRepository)
{
    public async Task<Result> Handle(DeleteTopicCommand command, CancellationToken cancellationToken)
    {
        var topic = await topicRepository.GetByIdAsync(command.OwnerId, command.TopicId, cancellationToken);
        if (topic is null)
        {
            return Error.NotFound("topics.not_found", "Topic was not found.");
        }

        if (await topicRepository.HasChildrenAsync(command.OwnerId, command.TopicId, cancellationToken))
        {
            return Error.Conflict("topics.has_children", "Delete or move subtopics before deleting this topic.");
        }

        await topicRepository.DeleteAsync(command.OwnerId, command.TopicId, cancellationToken);
        return Result.Success();
    }
}
