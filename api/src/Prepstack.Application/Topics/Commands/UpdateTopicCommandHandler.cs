using Prepstack.Domain.Common;
using Prepstack.Domain.Topics;

namespace Prepstack.Application.Topics.Commands;

public sealed class UpdateTopicCommandHandler(ITopicRepository topicRepository, TimeProvider timeProvider)
{
    public async Task<Result<Topic>> Handle(UpdateTopicCommand command, CancellationToken cancellationToken)
    {
        var topic = await topicRepository.GetByIdAsync(command.OwnerId, command.TopicId, cancellationToken);
        if (topic is null)
        {
            return Error.NotFound("topics.not_found", "Topic was not found.");
        }

        topic.UpdateDetails(command.Name, command.Description, command.Visibility, timeProvider.GetUtcNow());
        await topicRepository.UpdateAsync(topic, cancellationToken);

        return Result<Topic>.Success(topic);
    }
}
