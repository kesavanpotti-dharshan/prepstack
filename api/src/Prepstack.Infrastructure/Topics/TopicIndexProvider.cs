using MongoDB.Driver;
using Prepstack.Infrastructure.Mongo;

namespace Prepstack.Infrastructure.Topics;

public sealed class TopicIndexProvider : IMongoIndexProvider
{
    public Task CreateIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
    {
        var collection = database.GetCollection<TopicDocument>("topics");

        var uniquePath = new CreateIndexModel<TopicDocument>(
            Builders<TopicDocument>.IndexKeys.Ascending(t => t.OwnerId).Ascending(t => t.Path),
            new CreateIndexOptions { Unique = true, Name = "ux_topics_owner_path" });

        var ownerParent = new CreateIndexModel<TopicDocument>(
            Builders<TopicDocument>.IndexKeys.Ascending(t => t.OwnerId).Ascending(t => t.ParentId),
            new CreateIndexOptions { Name = "ix_topics_owner_parent" });

        return collection.Indexes.CreateManyAsync([uniquePath, ownerParent], cancellationToken);
    }
}
