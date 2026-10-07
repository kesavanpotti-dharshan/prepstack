using Microsoft.Extensions.DependencyInjection;
using Prepstack.Application.Auth;
using Prepstack.Infrastructure.Mongo;

namespace Prepstack.Infrastructure.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IUserRepository, MongoUserRepository>();
        services.AddScoped<IRefreshTokenRepository, MongoRefreshTokenRepository>();
        services.AddSingleton<IMongoIndexProvider, UserIndexProvider>();
        services.AddSingleton<IMongoIndexProvider, RefreshTokenIndexProvider>();

        return services;
    }
}
