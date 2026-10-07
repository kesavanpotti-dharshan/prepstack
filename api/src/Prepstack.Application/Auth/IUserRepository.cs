using Prepstack.Domain.Auth;

namespace Prepstack.Application.Auth;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);
}
