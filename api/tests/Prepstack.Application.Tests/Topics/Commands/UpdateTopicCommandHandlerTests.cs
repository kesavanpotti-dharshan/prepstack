using NSubstitute;
using Prepstack.Application.Topics;
using Prepstack.Application.Topics.Commands;
using Prepstack.Domain.Common;
using Prepstack.Domain.Topics;
using Shouldly;

namespace Prepstack.Application.Tests.Topics.Commands;

public class UpdateTopicCommandHandlerTests
{
    private readonly ITopicRepository _topicRepository = Substitute.For<ITopicRepository>();

    private UpdateTopicCommandHandler CreateHandler() => new(_topicRepository, TimeProvider.System);

    [Fact]
    public async Task Handle_ExistingTopic_UpdatesDetailsButKeepsPath()
    {
        var topic = Topic.Create("topic-1", "owner-1", "Original", null, null, null, "private", DateTimeOffset.UtcNow);
        _topicRepository.GetByIdAsync("owner-1", "topic-1", Arg.Any<CancellationToken>()).Returns(topic);

        var result = await CreateHandler().Handle(
            new UpdateTopicCommand("owner-1", "topic-1", "Renamed", "notes", "public"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Renamed");
        result.Value.Description.ShouldBe("notes");
        result.Value.Visibility.ShouldBe("public");
        result.Value.Path.ShouldBe("original");
        await _topicRepository.Received(1).UpdateAsync(topic, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TopicNotFoundOrOwnedByAnotherUser_ReturnsNotFound()
    {
        _topicRepository.GetByIdAsync("owner-1", "topic-1", Arg.Any<CancellationToken>()).Returns((Topic?)null);

        var result = await CreateHandler().Handle(
            new UpdateTopicCommand("owner-1", "topic-1", "Renamed", null, "private"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.NotFound);
        result.Error!.Code.ShouldBe("topics.not_found");
    }
}
