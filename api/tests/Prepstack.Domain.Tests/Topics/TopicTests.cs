using Prepstack.Domain.Topics;
using Shouldly;

namespace Prepstack.Domain.Tests.Topics;

public class TopicTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_RootTopic_PathEqualsSlug()
    {
        var topic = Topic.Create("id-1", "owner-1", ".NET Dependency Injection", null, null, null, "private", Now);

        topic.Slug.ShouldBe("net-dependency-injection");
        topic.Path.ShouldBe("net-dependency-injection");
        topic.ParentId.ShouldBeNull();
    }

    [Fact]
    public void Create_ChildTopic_PathIsParentPathSlashSlug()
    {
        var topic = Topic.Create("id-2", "owner-1", "DI", "parent-id", "dotnet", null, "private", Now);

        topic.Slug.ShouldBe("di");
        topic.Path.ShouldBe("dotnet/di");
        topic.ParentId.ShouldBe("parent-id");
    }

    [Fact]
    public void Create_NewTopic_HasZeroQuestionCount()
    {
        var topic = Topic.Create("id-1", "owner-1", "Topic", null, null, null, "private", Now);

        topic.QuestionCount.ShouldBe(0);
    }

    [Fact]
    public void Create_BlankDescription_NormalizesToNull()
    {
        var topic = Topic.Create("id-1", "owner-1", "Topic", null, null, "   ", "private", Now);

        topic.Description.ShouldBeNull();
    }

    [Fact]
    public void Create_TrimsDescription()
    {
        var topic = Topic.Create("id-1", "owner-1", "Topic", null, null, "  notes  ", "private", Now);

        topic.Description.ShouldBe("notes");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankName_Throws(string name)
    {
        Should.Throw<ArgumentException>(() => Topic.Create("id-1", "owner-1", name, null, null, null, "private", Now));
    }

    [Fact]
    public void Create_NameWithOnlyPunctuation_Throws()
    {
        Should.Throw<ArgumentException>(() => Topic.Create("id-1", "owner-1", "---", null, null, null, "private", Now));
    }

    [Theory]
    [InlineData("secret")]
    [InlineData("")]
    public void Create_InvalidVisibility_Throws(string visibility)
    {
        Should.Throw<ArgumentException>(() => Topic.Create("id-1", "owner-1", "Topic", null, null, null, visibility, Now));
    }

    [Fact]
    public void UpdateDetails_ChangesNameDescriptionAndVisibility_ButNotSlugOrPath()
    {
        var topic = Topic.Create("id-1", "owner-1", "Original Name", null, null, null, "private", Now);
        var later = Now.AddDays(1);

        topic.UpdateDetails("Renamed Topic", "new notes", "public", later);

        topic.Name.ShouldBe("Renamed Topic");
        topic.Description.ShouldBe("new notes");
        topic.Visibility.ShouldBe("public");
        topic.UpdatedAt.ShouldBe(later);
        topic.Slug.ShouldBe("original-name");
        topic.Path.ShouldBe("original-name");
    }

    [Fact]
    public void UpdateDetails_BlankName_Throws()
    {
        var topic = Topic.Create("id-1", "owner-1", "Original Name", null, null, null, "private", Now);

        Should.Throw<ArgumentException>(() => topic.UpdateDetails("", null, "private", Now));
    }
}
