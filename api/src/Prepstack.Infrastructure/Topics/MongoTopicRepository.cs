using MongoDB.Driver;
using Prepstack.Application.Topics;
using Prepstack.Domain.Topics;

namespace Prepstack.Infrastructure.Topics;

public sealed class MongoTopicRepository(IMongoDatabase database) : ITopicRepository
{
    private readonly IMongoCollection<TopicDocument> _topics = database.GetCollection<TopicDocument>("topics");

    public async Task<Topic?> GetByIdAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        var filter = OwnerFilter(ownerId) & Builders<TopicDocument>.Filter.Eq(t => t.Id, id);
        var document = await _topics.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document?.ToDomain();
    }

    public async Task<bool> HasChildrenAsync(string ownerId, string parentId, CancellationToken cancellationToken)
    {
        var filter = OwnerFilter(ownerId) & Builders<TopicDocument>.Filter.Eq(t => t.ParentId, parentId);
        return await _topics.Find(filter).AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByPathAsync(string ownerId, string path, CancellationToken cancellationToken)
    {
        var filter = OwnerFilter(ownerId) & Builders<TopicDocument>.Filter.Eq(t => t.Path, path);
        return await _topics.Find(filter).AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Topic>> ListAllAsync(string ownerId, CancellationToken cancellationToken)
    {
        var documents = await _topics.Find(OwnerFilter(ownerId))
            .SortBy(t => t.Path)
            .ToListAsync(cancellationToken);
        return [.. documents.Select(d => d.ToDomain())];
    }

    public Task AddAsync(Topic topic, CancellationToken cancellationToken) =>
        _topics.InsertOneAsync(TopicDocument.FromDomain(topic), options: null, cancellationToken);

    public async Task UpdateAsync(Topic topic, CancellationToken cancellationToken)
    {
        var filter = OwnerFilter(topic.OwnerId) & Builders<TopicDocument>.Filter.Eq(t => t.Id, topic.Id);
        var update = Builders<TopicDocument>.Update
            .Set(t => t.Name, topic.Name)
            .Set(t => t.Description, topic.Description)
            .Set(t => t.Visibility, topic.Visibility)
            .Set(t => t.UpdatedAt, topic.UpdatedAt);

        await _topics.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string ownerId, string id, CancellationToken cancellationToken)
    {
        var filter = OwnerFilter(ownerId) & Builders<TopicDocument>.Filter.Eq(t => t.Id, id);
        await _topics.DeleteOneAsync(filter, cancellationToken);
    }

    private static FilterDefinition<TopicDocument> OwnerFilter(string ownerId) =>
        Builders<TopicDocument>.Filter.Eq(t => t.OwnerId, ownerId);
}
