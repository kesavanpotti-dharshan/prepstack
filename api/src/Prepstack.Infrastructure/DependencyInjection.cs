using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prepstack.Application.Common;
using Prepstack.Infrastructure.Auth;
using Prepstack.Infrastructure.Common;
using Prepstack.Infrastructure.Mongo;

namespace Prepstack.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongo(configuration);
        services.AddSingleton<IIdGenerator, ObjectIdGenerator>();
        services.AddAuthInfrastructure();

        return services;
    }
}
