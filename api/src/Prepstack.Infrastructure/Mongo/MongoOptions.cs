namespace Prepstack.Infrastructure.Mongo;

public sealed class MongoOptions
{
    public const string SectionName = "Mongo";

    public required string ConnectionString { get; init; }

    public required string DatabaseName { get; init; }
}
