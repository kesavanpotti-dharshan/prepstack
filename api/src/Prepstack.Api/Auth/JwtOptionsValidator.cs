using System.Text;
using Microsoft.Extensions.Options;

namespace Prepstack.Api.Auth;

internal sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            return ValidateOptionsResult.Fail($"{nameof(JwtOptions.Issuer)} is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            return ValidateOptionsResult.Fail($"{nameof(JwtOptions.Audience)} is required.");
        }

        if (Encoding.UTF8.GetByteCount(options.SigningKey ?? string.Empty) < 32)
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(JwtOptions.SigningKey)} must be at least 32 bytes (HMAC-SHA256 key size).");
        }

        if (options.AccessTokenLifetimeMinutes <= 0)
        {
            return ValidateOptionsResult.Fail($"{nameof(JwtOptions.AccessTokenLifetimeMinutes)} must be positive.");
        }

        return ValidateOptionsResult.Success;
    }
}
