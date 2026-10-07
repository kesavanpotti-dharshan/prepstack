using MongoDB.Bson;
using Prepstack.Application.Common;

namespace Prepstack.Infrastructure.Common;

public sealed class ObjectIdGenerator : IIdGenerator
{
    public string NewId() => ObjectId.GenerateNewId().ToString();
}
