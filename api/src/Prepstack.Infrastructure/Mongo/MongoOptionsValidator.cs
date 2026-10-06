using Microsoft.Extensions.Options;

namespace Prepstack.Infrastructure.Mongo;

internal sealed class MongoOptionsValidator : IValidateOptions<MongoOptions>
{
    public ValidateOptionsResult Validate(string? name, MongoOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail($"{nameof(MongoOptions.ConnectionString)} is required.");
        }

        if (string.IsNullOrWhiteSpace(options.DatabaseName))
        {
            return ValidateOptionsResult.Fail($"{nameof(MongoOptions.DatabaseName)} is required.");
        }

        return ValidateOptionsResult.Success;
    }
}
