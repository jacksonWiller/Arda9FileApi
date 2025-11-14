using System.Text.RegularExpressions;

namespace Arda9FileApi.Domain.ValueObjects;

/// <summary>
/// Value Object for Slug: ^[a-z0-9]+(?:-[a-z0-9]+)*$, 3-60 chars, globally unique
/// </summary>
public class Slug
{
    private static readonly Regex SlugRegex = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public string Value { get; private set; }

    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Slug cannot be empty");

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length < 3 || normalized.Length > 60)
            throw new ArgumentException("Slug must be between 3 and 60 characters");

        if (!SlugRegex.IsMatch(normalized))
            throw new ArgumentException("Slug must match pattern: lowercase letters, numbers, and hyphens only");

        Value = normalized;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is Slug other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
