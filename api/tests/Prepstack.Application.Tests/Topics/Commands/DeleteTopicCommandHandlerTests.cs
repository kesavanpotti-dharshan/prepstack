using NSubstitute;
using Prepstack.Application.Topics;
using Prepstack.Application.Topics.Commands;
using Prepstack.Domain.Common;
using Prepstack.Domain.Topics;
using Shouldly;

namespace Prepstack.Application.Tests.Topics.Commands;

public class DeleteTopicCommandHandlerTests
{
    private readonly ITopicRepository _topicRepository = Substitute.For<ITopicRepository>();

    private DeleteTopicCommandHandler CreateHandler() => new(_topicRepository);

    [Fact]
    public async Task Handle_LeafTopic_Deletes()
    {
        var topic = Topic.Create("topic-1", "owner-1", "Leaf", null, null, null, "private", DateTimeOffset.UtcNow);
        _topicRepository.GetByIdAsync("owner-1", "topic-1", Arg.Any<CancellationToken>()).Returns(topic);
        _topicRepository.HasChildrenAsync("owner-1", "topic-1", Arg.Any<CancellationToken>()).Returns(false);

        var result = await CreateHandler().Handle(new DeleteTopicCommand("owner-1", "topic-1"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _topicRepository.Received(1).DeleteAsync("owner-1", "topic-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TopicHasChildren_ReturnsConflictAndDoesNotDelete()
    {
        var topic = Topic.Create("topic-1", "owner-1", "Parent", null, null, null, "private", DateTimeOffset.UtcNow);
        _topicRepository.GetByIdAsync("owner-1", "topic-1", Arg.Any<CancellationToken>()).Returns(topic);
        _topicRepository.HasChildrenAsync("owner-1", "topic-1", Arg.Any<CancellationToken>()).Returns(true);

        var result = await CreateHandler().Handle(new DeleteTopicCommand("owner-1", "topic-1"), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Conflict);
        result.Error!.Code.ShouldBe("topics.has_children");
        await _topicRepository.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TopicNotFoundOrOwnedByAnotherUser_ReturnsNotFound()
    {
        _topicRepository.GetByIdAsync("owner-1", "topic-1", Arg.Any<CancellationToken>()).Returns((Topic?)null);

        var result = await CreateHandler().Handle(new DeleteTopicCommand("owner-1", "topic-1"), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("topics.not_found");
    }
}
