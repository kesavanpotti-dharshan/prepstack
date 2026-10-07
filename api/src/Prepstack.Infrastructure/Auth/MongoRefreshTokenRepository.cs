using MongoDB.Driver;
using Prepstack.Application.Auth;
using Prepstack.Domain.Auth;

namespace Prepstack.Infrastructure.Auth;

public sealed class MongoRefreshTokenRepository(IMongoDatabase database) : IRefreshTokenRepository
{
    private readonly IMongoClient _client = database.Client;
    private readonly IMongoCollection<RefreshTokenDocument> _tokens =
        database.GetCollection<RefreshTokenDocument>("refreshTokens");

    public Task AddAsync(RefreshToken token, CancellationToken cancellationToken) =>
        _tokens.InsertOneAsync(RefreshTokenDocument.FromDomain(token), options: null, cancellationToken);

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var filter = Builders<RefreshTokenDocument>.Filter.Eq(t => t.TokenHash, tokenHash);
        var document = await _tokens.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document?.ToDomain();
    }

    public async Task RotateAsync(RefreshToken revoked, RefreshToken replacement, CancellationToken cancellationToken)
    {
        using var session = await _client.StartSessionAsync(cancellationToken: cancellationToken);

        await session.WithTransactionAsync(
            async (handle, ct) =>
            {
                var filter = Builders<RefreshTokenDocument>.Filter.Eq(t => t.Id, revoked.Id);
                var update = Builders<RefreshTokenDocument>.Update
                    .Set(t => t.RevokedAt, revoked.RevokedAt)
                    .Set(t => t.ReplacedBy, revoked.ReplacedBy);

                await _tokens.UpdateOneAsync(handle, filter, update, cancellationToken: ct);
                await _tokens.InsertOneAsync(handle, RefreshTokenDocument.FromDomain(replacement), cancellationToken: ct);
                return true;
            },
            cancellationToken: cancellationToken);
    }

    public async Task RevokeFamilyAsync(string familyId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var filter = Builders<RefreshTokenDocument>.Filter.And(
            Builders<RefreshTokenDocument>.Filter.Eq(t => t.FamilyId, familyId),
            Builders<RefreshTokenDocument>.Filter.Eq(t => t.RevokedAt, null));
        var update = Builders<RefreshTokenDocument>.Update.Set(t => t.RevokedAt, now);

        await _tokens.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task RevokeAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        var filter = Builders<RefreshTokenDocument>.Filter.Eq(t => t.Id, token.Id);
        var update = Builders<RefreshTokenDocument>.Update.Set(t => t.RevokedAt, token.RevokedAt);

        await _tokens.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}
