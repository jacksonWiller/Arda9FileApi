using System.Text.RegularExpressions;

namespace Arda9FileApi.Domain.ValueObjects;

/// <summary>
/// Value Object for URL: valid HTTP/HTTPS URL
/// </summary>
public class Url
{
    private static readonly Regex UrlRegex = new(
        @"^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string Value { get; private set; }

    public Url(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("URL cannot be null or whitespace");

        var trimmed = value.Trim();

        if (!UrlRegex.IsMatch(trimmed))
            throw new ArgumentException("Invalid URL format");

        if (trimmed.Length > 2048)
            throw new ArgumentException("URL cannot exceed 2048 characters");

        Value = trimmed;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        if (obj is Url other)
            return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
        return false;
    }

    public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();
}