using MongoDB.Driver;
using Prepstack.Infrastructure.Mongo;

namespace Prepstack.Infrastructure.Auth;

public sealed class RefreshTokenIndexProvider : IMongoIndexProvider
{
    public Task CreateIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
    {
        var collection = database.GetCollection<RefreshTokenDocument>("refreshTokens");

        var tokenHashIndex = new CreateIndexModel<RefreshTokenDocument>(
            Builders<RefreshTokenDocument>.IndexKeys.Ascending(t => t.TokenHash),
            new CreateIndexOptions { Unique = true, Name = "ux_refreshTokens_tokenHash" });

        var expiresAtIndex = new CreateIndexModel<RefreshTokenDocument>(
            Builders<RefreshTokenDocument>.IndexKeys.Ascending(t => t.ExpiresAt),
            new CreateIndexOptions { ExpireAfter = TimeSpan.Zero, Name = "ttl_refreshTokens_expiresAt" });

        return collection.Indexes.CreateManyAsync([tokenHashIndex, expiresAtIndex], cancellationToken);
    }
}
