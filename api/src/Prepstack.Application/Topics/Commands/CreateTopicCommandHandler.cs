using Prepstack.Application.Common;
using Prepstack.Domain.Common;
using Prepstack.Domain.Topics;

namespace Prepstack.Application.Topics.Commands;

public sealed class CreateTopicCommandHandler(
    ITopicRepository topicRepository,
    IIdGenerator idGenerator,
    TimeProvider timeProvider)
{
    public async Task<Result<Topic>> Handle(CreateTopicCommand command, CancellationToken cancellationToken)
    {
        string? parentPath = null;
        if (command.ParentId is not null)
        {
            var parent = await topicRepository.GetByIdAsync(command.OwnerId, command.ParentId, cancellationToken);
            if (parent is null)
            {
                return Error.NotFound("topics.parent_not_found", "Parent topic was not found.");
            }

            parentPath = parent.Path;
        }

        var now = timeProvider.GetUtcNow();
        var topic = Topic.Create(
            idGenerator.NewId(),
            command.OwnerId,
            command.Name,
            command.ParentId,
            parentPath,
            command.Description,
            command.Visibility,
            now);

        if (await topicRepository.ExistsByPathAsync(command.OwnerId, topic.Path, cancellationToken))
        {
            return Error.Conflict("topics.duplicate_path", "A topic with this name already exists under the same parent.");
        }

        await topicRepository.AddAsync(topic, cancellationToken);
        return Result<Topic>.Success(topic);
    }
}
