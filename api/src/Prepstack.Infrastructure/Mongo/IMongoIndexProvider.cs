using MongoDB.Driver;

namespace Prepstack.Infrastructure.Mongo;

/// <summary>
/// One per collection. Implementations declare the indexes that collection needs;
/// <see cref="MongoIndexInitializer"/> applies them all on startup (rules.md → MongoDB).
/// </summary>
public interface IMongoIndexProvider
{
    Task CreateIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken);
}
