using Prepstack.Domain.Common;
using Prepstack.Domain.Topics;

namespace Prepstack.Application.Topics.Queries;

public sealed class GetTopicTreeQueryHandler(ITopicRepository topicRepository)
{
    public async Task<Result<IReadOnlyList<Topic>>> Handle(GetTopicTreeQuery query, CancellationToken cancellationToken)
    {
        var topics = await topicRepository.ListAllAsync(query.OwnerId, cancellationToken);
        return Result<IReadOnlyList<Topic>>.Success(topics);
    }
}
