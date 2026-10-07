using MongoDB.Driver;
using Prepstack.Infrastructure.Mongo;

namespace Prepstack.Infrastructure.Auth;

public sealed class UserIndexProvider : IMongoIndexProvider
{
    public Task CreateIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
    {
        var collection = database.GetCollection<UserDocument>("users");
        var model = new CreateIndexModel<UserDocument>(
            Builders<UserDocument>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true, Name = "ux_users_email" });

        return collection.Indexes.CreateOneAsync(model, cancellationToken: cancellationToken);
    }
}
