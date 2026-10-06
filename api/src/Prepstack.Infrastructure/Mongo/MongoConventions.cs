using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;

namespace Prepstack.Infrastructure.Mongo;

/// <summary>
/// Registers the BSON class-map conventions shared by every document in the bank
/// (camelCase fields, enums as strings per rules.md, tolerant reads).
/// </summary>
public static class MongoConventions
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new EnumRepresentationConvention(BsonType.String),
            new IgnoreExtraElementsConvention(true),
        };

        ConventionRegistry.Register("prepstack-conventions", pack, _ => true);
        _registered = true;
    }
}
