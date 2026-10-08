using FluentValidation;
using Prepstack.Api.Common;
using Prepstack.Application.Topics.Commands;
using Prepstack.Application.Topics.Queries;
using Prepstack.Domain.Topics;

namespace Prepstack.Api.Topics;

public static class TopicsEndpoints
{
    public static void MapTopicsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/topics").RequireAuthorization();

        group.MapGet("", GetTreeAsync)
            .Produces<IReadOnlyList<TopicTreeNodeResponse>>(StatusCodes.Status200OK);

        group.MapPost("", CreateAsync)
            .Produces<TopicResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{id}", UpdateAsync)
            .Produces<TopicResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id}", DeleteAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetTreeAsync(
        HttpContext httpContext,
        GetTopicTreeQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var ownerId = httpContext.User.GetOwnerId();
        var result = await handler.Handle(new GetTopicTreeQuery(ownerId), cancellationToken);
        return Results.Ok(BuildTree(result.Value));
    }

    private static async Task<IResult> CreateAsync(
        CreateTopicRequest request,
        HttpContext httpContext,
        IValidator<CreateTopicCommand> validator,
        CreateTopicCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateTopicCommand(
            httpContext.User.GetOwnerId(),
            request.Name,
            request.ParentId,
            request.Description,
            request.Visibility ?? "private");

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToValidationProblem();
        }

        var result = await handler.Handle(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error!.ToProblem();
        }

        var response = ToResponse(result.Value);
        return Results.Created($"/api/v1/topics/{response.Id}", response);
    }

    private static async Task<IResult> UpdateAsync(
        string id,
        UpdateTopicRequest request,
        HttpContext httpContext,
        IValidator<UpdateTopicCommand> validator,
        UpdateTopicCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTopicCommand(
            httpContext.User.GetOwnerId(),
            id,
            request.Name,
            request.Description,
            request.Visibility);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToValidationProblem();
        }

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(ToResponse(result.Value)) : result.Error!.ToProblem();
    }

    private static async Task<IResult> DeleteAsync(
        string id,
        HttpContext httpContext,
        DeleteTopicCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTopicCommand(httpContext.User.GetOwnerId(), id);
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();
    }

    private static TopicResponse ToResponse(Topic topic) => new(
        topic.Id,
        topic.Slug,
        topic.Name,
        topic.ParentId,
        topic.Path,
        topic.Description,
        topic.Visibility,
        topic.QuestionCount,
        topic.CreatedAt,
        topic.UpdatedAt);

    private static IReadOnlyList<TopicTreeNodeResponse> BuildTree(IReadOnlyList<Topic> topics)
    {
        var childrenByParentId = topics.Where(t => t.ParentId is not null).ToLookup(t => t.ParentId!);

        TopicTreeNodeResponse ToNode(Topic topic) => new(
            topic.Id,
            topic.Slug,
            topic.Name,
            topic.ParentId,
            topic.Path,
            topic.Description,
            topic.Visibility,
            topic.QuestionCount,
            topic.CreatedAt,
            topic.UpdatedAt,
            [.. childrenByParentId[topic.Id].Select(ToNode)]);

        return [.. topics.Where(t => t.ParentId is null).Select(ToNode)];
    }
}
