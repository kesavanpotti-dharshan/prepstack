using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Prepstack.Application.Auth;

namespace Prepstack.Infrastructure.Auth;

/// <summary>
/// OWASP's low-memory Argon2id minimum (19 MiB / t=2 / p=1) — chosen because this runs
/// on an Azure Container Apps Consumption (scale-to-zero, free-tier) instance, not a
/// dedicated auth server with memory to spare.
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const string Prefix = "argon2id";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int MemoryKb = 19_456;
    private const int Iterations = 2;
    private const int Parallelism = 1;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = ComputeHash(password, salt, HashSize);
        return $"{Prefix}$v=19$m={MemoryKb},t={Iterations},p={Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('$');
        if (parts.Length != 5 || parts[0] != Prefix)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[3]);
        var expectedHash = Convert.FromBase64String(parts[4]);
        var actualHash = ComputeHash(password, salt, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static byte[] ComputeHash(string password, byte[] salt, int hashSize)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = Parallelism,
            Iterations = Iterations,
            MemorySize = MemoryKb,
        };

        return argon2.GetBytes(hashSize);
    }
}
