using System.Security.Cryptography;
using System.Text;
using Prepstack.Application.Auth;

namespace Prepstack.Infrastructure.Auth;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int SecretSizeBytes = 32;

    public RefreshTokenSecret Generate()
    {
        var raw = Base64UrlEncode(RandomNumberGenerator.GetBytes(SecretSizeBytes));
        return new RefreshTokenSecret(raw, Hash(raw));
    }

    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
