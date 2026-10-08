using Microsoft.Extensions.DependencyInjection;
using Prepstack.Application.Topics;
using Prepstack.Infrastructure.Mongo;

namespace Prepstack.Infrastructure.Topics;

public static class TopicsServiceCollectionExtensions
{
    public static IServiceCollection AddTopicsInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ITopicRepository, MongoTopicRepository>();
        services.AddSingleton<IMongoIndexProvider, TopicIndexProvider>();

        return services;
    }
}
