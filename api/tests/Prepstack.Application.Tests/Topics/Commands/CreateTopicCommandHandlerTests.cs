using NSubstitute;
using Prepstack.Application.Common;
using Prepstack.Application.Topics;
using Prepstack.Application.Topics.Commands;
using Prepstack.Domain.Common;
using Prepstack.Domain.Topics;
using Shouldly;

namespace Prepstack.Application.Tests.Topics.Commands;

public class CreateTopicCommandHandlerTests
{
    private readonly ITopicRepository _topicRepository = Substitute.For<ITopicRepository>();
    private readonly IIdGenerator _idGenerator = Substitute.For<IIdGenerator>();

    private CreateTopicCommandHandler CreateHandler() =>
        new(_topicRepository, _idGenerator, TimeProvider.System);

    [Fact]
    public async Task Handle_RootTopic_CreatesWithPathEqualToSlug()
    {
        _idGenerator.NewId().Returns("topic-1");
        _topicRepository.ExistsByPathAsync("owner-1", "dotnet", Arg.Any<CancellationToken>()).Returns(false);

        var result = await CreateHandler().Handle(
            new CreateTopicCommand("owner-1", "Dotnet", null, null, "private"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Path.ShouldBe("dotnet");
        await _topicRepository.Received(1).AddAsync(
            Arg.Is<Topic>(t => t.Id == "topic-1" && t.OwnerId == "owner-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ChildTopic_PrependsParentPath()
    {
        var parent = Topic.Create("parent-1", "owner-1", "Dotnet", null, null, null, "private", DateTimeOffset.UtcNow);
        _idGenerator.NewId().Returns("topic-2");
        _topicRepository.GetByIdAsync("owner-1", "parent-1", Arg.Any<CancellationToken>()).Returns(parent);
        _topicRepository.ExistsByPathAsync("owner-1", "dotnet/di", Arg.Any<CancellationToken>()).Returns(false);

        var result = await CreateHandler().Handle(
            new CreateTopicCommand("owner-1", "DI", "parent-1", null, "private"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Path.ShouldBe("dotnet/di");
        result.Value.ParentId.ShouldBe("parent-1");
    }

    [Fact]
    public async Task Handle_ParentNotFound_ReturnsNotFound()
    {
        _topicRepository.GetByIdAsync("owner-1", "missing-parent", Arg.Any<CancellationToken>()).Returns((Topic?)null);

        var result = await CreateHandler().Handle(
            new CreateTopicCommand("owner-1", "DI", "missing-parent", null, "private"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.NotFound);
        result.Error!.Code.ShouldBe("topics.parent_not_found");
    }

    [Fact]
    public async Task Handle_DuplicatePath_ReturnsConflict()
    {
        _idGenerator.NewId().Returns("topic-1");
        _topicRepository.ExistsByPathAsync("owner-1", "dotnet", Arg.Any<CancellationToken>()).Returns(true);

        var result = await CreateHandler().Handle(
            new CreateTopicCommand("owner-1", "Dotnet", null, null, "private"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Type.ShouldBe(ErrorType.Conflict);
        result.Error!.Code.ShouldBe("topics.duplicate_path");
        await _topicRepository.DidNotReceive().AddAsync(Arg.Any<Topic>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ParentOwnedByAnotherUser_ReturnsNotFound()
    {
        // GetByIdAsync is scoped by ownerId, so another owner's topic id simply won't be found.
        _topicRepository.GetByIdAsync("owner-1", "other-owners-topic", Arg.Any<CancellationToken>()).Returns((Topic?)null);

        var result = await CreateHandler().Handle(
            new CreateTopicCommand("owner-1", "DI", "other-owners-topic", null, "private"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("topics.parent_not_found");
    }
}
