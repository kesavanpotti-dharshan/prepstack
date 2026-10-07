namespace Prepstack.Domain.Auth;

public sealed class User
{
    public string Id { get; }

    public string Email { get; }

    public string PasswordHash { get; }

    public string DisplayName { get; }

    public IReadOnlyList<string> Roles { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; }

    private User(
        string id,
        string email,
        string passwordHash,
        string displayName,
        IReadOnlyList<string> roles,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        DisplayName = displayName;
        Roles = roles;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static User Register(string id, string email, string passwordHash, string displayName, DateTimeOffset now)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (normalizedEmail.Length == 0)
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        var trimmedDisplayName = displayName.Trim();
        if (trimmedDisplayName.Length == 0)
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        return new User(id, normalizedEmail, passwordHash, trimmedDisplayName, [], now, now);
    }

    public static User Reconstitute(
        string id,
        string email,
        string passwordHash,
        string displayName,
        IReadOnlyList<string> roles,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        => new(id, email, passwordHash, displayName, roles, createdAt, updatedAt);

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
