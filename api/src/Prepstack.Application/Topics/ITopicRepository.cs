using Prepstack.Domain.Topics;

namespace Prepstack.Application.Topics;

public interface ITopicRepository
{
    Task<Topic?> GetByIdAsync(string ownerId, string id, CancellationToken cancellationToken);

    Task<bool> HasChildrenAsync(string ownerId, string parentId, CancellationToken cancellationToken);

    Task<bool> ExistsByPathAsync(string ownerId, string path, CancellationToken cancellationToken);

    /// <summary>Flat list for one owner, sorted by <see cref="Topic.Path"/> so parents precede children.</summary>
    Task<IReadOnlyList<Topic>> ListAllAsync(string ownerId, CancellationToken cancellationToken);

    Task AddAsync(Topic topic, CancellationToken cancellationToken);

    Task UpdateAsync(Topic topic, CancellationToken cancellationToken);

    Task DeleteAsync(string ownerId, string id, CancellationToken cancellationToken);
}
