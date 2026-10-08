using System.Text;

namespace Prepstack.Domain.Topics;

public sealed class Topic
{
    private static readonly HashSet<string> AllowedVisibilities = ["private", "public"];

    public string Id { get; }

    public string OwnerId { get; }

    public string Slug { get; }

    public string Name { get; private set; }

    public string? ParentId { get; }

    public string Path { get; }

    public string? Description { get; private set; }

    public string Visibility { get; private set; }

    public int QuestionCount { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    private Topic(
        string id,
        string ownerId,
        string slug,
        string name,
        string? parentId,
        string path,
        string? description,
        string visibility,
        int questionCount,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        OwnerId = ownerId;
        Slug = slug;
        Name = name;
        ParentId = parentId;
        Path = path;
        Description = description;
        Visibility = visibility;
        QuestionCount = questionCount;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Topic Create(
        string id,
        string ownerId,
        string name,
        string? parentId,
        string? parentPath,
        string? description,
        string visibility,
        DateTimeOffset now)
    {
        var trimmedName = ValidateName(name);
        var slug = Slugify(trimmedName);
        if (slug.Length == 0)
        {
            throw new ArgumentException("Name must contain at least one letter or digit.", nameof(name));
        }

        ValidateVisibility(visibility);

        var path = parentPath is null ? slug : $"{parentPath}/{slug}";

        return new Topic(
            id,
            ownerId,
            slug,
            trimmedName,
            parentId,
            path,
            NormalizeDescription(description),
            visibility,
            questionCount: 0,
            now,
            now);
    }

    public static Topic Reconstitute(
        string id,
        string ownerId,
        string slug,
        string name,
        string? parentId,
        string path,
        string? description,
        string visibility,
        int questionCount,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        => new(id, ownerId, slug, name, parentId, path, description, visibility, questionCount, createdAt, updatedAt);

    /// <summary>Updates display fields only — slug/path are immutable once created (see docs/adr note in the Topics plan).</summary>
    public void UpdateDetails(string name, string? description, string visibility, DateTimeOffset now)
    {
        var trimmedName = ValidateName(name);
        ValidateVisibility(visibility);

        Name = trimmedName;
        Description = NormalizeDescription(description);
        Visibility = visibility;
        UpdatedAt = now;
    }

    private static string ValidateName(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return trimmed;
    }

    private static void ValidateVisibility(string visibility)
    {
        if (!AllowedVisibilities.Contains(visibility))
        {
            throw new ArgumentException("Visibility must be 'private' or 'public'.", nameof(visibility));
        }
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static string Slugify(string value)
    {
        var builder = new StringBuilder(value.Length);
        var lastWasHyphen = false;

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
                lastWasHyphen = false;
            }
            else if (!lastWasHyphen && builder.Length > 0)
            {
                builder.Append('-');
                lastWasHyphen = true;
            }
        }

        if (lastWasHyphen)
        {
            builder.Length--;
        }

        return builder.ToString();
    }
}
