using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Prepstack.Infrastructure.Mongo;

public sealed class MongoIndexInitializer(
    IMongoDatabase database,
    IEnumerable<IMongoIndexProvider> indexProviders,
    ILogger<MongoIndexInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var provider in indexProviders)
        {
            logger.LogInformation("Applying Mongo indexes for {IndexProvider}", provider.GetType().Name);
            await provider.CreateIndexesAsync(database, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
