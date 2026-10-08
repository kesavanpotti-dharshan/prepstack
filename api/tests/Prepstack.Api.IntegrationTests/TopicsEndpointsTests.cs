using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Prepstack.Api.IntegrationTests;

public sealed class TopicsEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Create_RootTopic_ReturnsCreatedWithPathEqualToSlug()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await CreateTopicAsync(client, "Dotnet");

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<TopicDto>();
        response.Headers.Location!.ToString().ShouldBe($"/api/v1/topics/{body!.Id}");
        body!.Path.ShouldBe("dotnet");
        body.Slug.ShouldBe("dotnet");
        body.Visibility.ShouldBe("private");
        body.QuestionCount.ShouldBe(0);
    }

    [Fact]
    public async Task Create_ChildTopic_PathIncludesParentPath()
    {
        var client = await CreateAuthenticatedClientAsync();
        var parent = await CreateTopicAndReadAsync(client, "Dotnet");

        var response = await CreateTopicAsync(client, "DI", parentId: parent.Id);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var child = await response.Content.ReadFromJsonAsync<TopicDto>();
        child!.Path.ShouldBe("dotnet/di");
        child.ParentId.ShouldBe(parent.Id);
    }

    [Fact]
    public async Task Create_DuplicateSlugUnderSameParent_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync();
        await CreateTopicAndReadAsync(client, "Dotnet");

        var response = await CreateTopicAsync(client, "Dotnet");

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_SameSlugUnderDifferentParents_Succeeds()
    {
        var client = await CreateAuthenticatedClientAsync();
        var dotnet = await CreateTopicAndReadAsync(client, "Dotnet");
        var java = await CreateTopicAndReadAsync(client, "Java");

        var underDotnet = await CreateTopicAsync(client, "Basics", parentId: dotnet.Id);
        var underJava = await CreateTopicAsync(client, "Basics", parentId: java.Id);

        underDotnet.StatusCode.ShouldBe(HttpStatusCode.Created);
        underJava.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ParentNotFound_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await CreateTopicAsync(client, "DI", parentId: RandomObjectId());

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_BlankName_ReturnsValidationProblem()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await CreateTopicAsync(client, "   ");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTree_ReturnsNestedTreeForCallerOnly()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var otherClient = await CreateAuthenticatedClientAsync();
        var parent = await CreateTopicAndReadAsync(ownerClient, "Dotnet");
        await CreateTopicAndReadAsync(ownerClient, "DI", parentId: parent.Id);

        var ownerTree = await ownerClient.GetFromJsonAsync<List<TopicTreeNodeDto>>("/api/v1/topics");
        var otherTree = await otherClient.GetFromJsonAsync<List<TopicTreeNodeDto>>("/api/v1/topics");

        ownerTree!.ShouldHaveSingleItem();
        ownerTree[0].Slug.ShouldBe("dotnet");
        ownerTree[0].Children.ShouldHaveSingleItem();
        ownerTree[0].Children[0].Slug.ShouldBe("di");
        otherTree.ShouldBeEmpty();
    }

    [Fact]
    public async Task Update_OwnTopic_UpdatesFieldsButKeepsPath()
    {
        var client = await CreateAuthenticatedClientAsync();
        var topic = await CreateTopicAndReadAsync(client, "Dotnet");

        var response = await client.PatchAsJsonAsync(
            $"/api/v1/topics/{topic.Id}",
            new { name = "Dotnet (renamed)", description = "notes", visibility = "public" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TopicDto>();
        updated!.Name.ShouldBe("Dotnet (renamed)");
        updated.Description.ShouldBe("notes");
        updated.Visibility.ShouldBe("public");
        updated.Path.ShouldBe("dotnet");
    }

    [Fact]
    public async Task Update_AnotherUsersTopic_ReturnsNotFound()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var otherClient = await CreateAuthenticatedClientAsync();
        var topic = await CreateTopicAndReadAsync(ownerClient, "Dotnet");

        var response = await otherClient.PatchAsJsonAsync(
            $"/api/v1/topics/{topic.Id}",
            new { name = "Hijacked", description = (string?)null, visibility = "public" });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var stillOwned = await ownerClient.GetFromJsonAsync<List<TopicTreeNodeDto>>("/api/v1/topics");
        stillOwned![0].Name.ShouldBe("Dotnet");
    }

    [Fact]
    public async Task Delete_LeafTopic_ReturnsNoContentAndRemovesFromTree()
    {
        var client = await CreateAuthenticatedClientAsync();
        var topic = await CreateTopicAndReadAsync(client, "Dotnet");

        var response = await client.DeleteAsync($"/api/v1/topics/{topic.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var tree = await client.GetFromJsonAsync<List<TopicTreeNodeDto>>("/api/v1/topics");
        tree.ShouldBeEmpty();
    }

    [Fact]
    public async Task Delete_TopicWithChildren_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync();
        var parent = await CreateTopicAndReadAsync(client, "Dotnet");
        await CreateTopicAndReadAsync(client, "DI", parentId: parent.Id);

        var response = await client.DeleteAsync($"/api/v1/topics/{parent.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_AnotherUsersTopic_ReturnsNotFoundAndLeavesItIntact()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var otherClient = await CreateAuthenticatedClientAsync();
        var topic = await CreateTopicAndReadAsync(ownerClient, "Dotnet");

        var response = await otherClient.DeleteAsync($"/api/v1/topics/{topic.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var stillThere = await ownerClient.GetFromJsonAsync<List<TopicTreeNodeDto>>("/api/v1/topics");
        stillThere.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Endpoints_WithoutBearerToken_ReturnUnauthorized()
    {
        var client = factory.CreateClient();

        (await client.GetAsync("/api/v1/topics")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await client.PostAsJsonAsync("/api/v1/topics", new { name = "Dotnet" }))
            .StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = UniqueEmail(), password = "password123", displayName = "Test User" });
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return client;
    }

    private static Task<HttpResponseMessage> CreateTopicAsync(HttpClient client, string name, string? parentId = null) =>
        client.PostAsJsonAsync(
            "/api/v1/topics",
            new { name, parentId, description = (string?)null, visibility = (string?)null });

    private static async Task<TopicDto> CreateTopicAndReadAsync(HttpClient client, string name, string? parentId = null)
    {
        var response = await CreateTopicAsync(client, name, parentId);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<TopicDto>())!;
    }

    private static string UniqueEmail() => $"{Guid.NewGuid():N}@example.com";

    private static string RandomObjectId() => Guid.NewGuid().ToString("N")[..24];

    private sealed record AuthResponseDto(string AccessToken, DateTimeOffset AccessTokenExpiresAt);

    private sealed record TopicDto(
        string Id,
        string Slug,
        string Name,
        string? ParentId,
        string Path,
        string? Description,
        string Visibility,
        int QuestionCount);

    private sealed record TopicTreeNodeDto(
        string Id,
        string Slug,
        string Name,
        string? ParentId,
        string Path,
        string? Description,
        string Visibility,
        int QuestionCount,
        List<TopicTreeNodeDto> Children);
}
