using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Prepstack.Application.Auth;
using Prepstack.Application.Auth.Commands;
using Prepstack.Application.Topics.Commands;
using Prepstack.Application.Topics.Queries;

namespace Prepstack.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthSessionIssuer>();
        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RefreshCommandHandler>();
        services.AddScoped<LogoutCommandHandler>();

        services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();
        services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
        services.AddScoped<IValidator<RefreshCommand>, RefreshCommandValidator>();

        services.AddScoped<CreateTopicCommandHandler>();
        services.AddScoped<UpdateTopicCommandHandler>();
        services.AddScoped<DeleteTopicCommandHandler>();
        services.AddScoped<GetTopicTreeQueryHandler>();

        services.AddScoped<IValidator<CreateTopicCommand>, CreateTopicCommandValidator>();
        services.AddScoped<IValidator<UpdateTopicCommand>, UpdateTopicCommandValidator>();

        return services;
    }
}
