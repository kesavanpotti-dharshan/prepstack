using MongoDB.Driver;
using Prepstack.Application.Auth;
using Prepstack.Domain.Auth;

namespace Prepstack.Infrastructure.Auth;

public sealed class MongoUserRepository(IMongoDatabase database) : IUserRepository
{
    private readonly IMongoCollection<UserDocument> _users = database.GetCollection<UserDocument>("users");

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var filter = Builders<UserDocument>.Filter.Eq(u => u.Email, email);
        var document = await _users.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document?.ToDomain();
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<UserDocument>.Filter.Eq(u => u.Id, id);
        var document = await _users.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document?.ToDomain();
    }

    public Task AddAsync(User user, CancellationToken cancellationToken) =>
        _users.InsertOneAsync(UserDocument.FromDomain(user), options: null, cancellationToken);
}
