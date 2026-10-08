using NSubstitute;
using Prepstack.Application.Topics;
using Prepstack.Application.Topics.Queries;
using Prepstack.Domain.Topics;
using Shouldly;

namespace Prepstack.Application.Tests.Topics.Queries;

public class GetTopicTreeQueryHandlerTests
{
    private readonly ITopicRepository _topicRepository = Substitute.For<ITopicRepository>();

    private GetTopicTreeQueryHandler CreateHandler() => new(_topicRepository);

    [Fact]
    public async Task Handle_ReturnsOwnersFlatTopicList()
    {
        var topics = new List<Topic>
        {
            Topic.Create("topic-1", "owner-1", "Dotnet", null, null, null, "private", DateTimeOffset.UtcNow),
        };
        _topicRepository.ListAllAsync("owner-1", Arg.Any<CancellationToken>()).Returns(topics);

        var result = await CreateHandler().Handle(new GetTopicTreeQuery("owner-1"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(topics);
    }
}
