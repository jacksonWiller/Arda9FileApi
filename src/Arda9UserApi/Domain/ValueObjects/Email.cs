using System.Text.RegularExpressions;

namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Value Object for Email: RFC 5322 format, lowercase, trimmed
/// </summary>
public class Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string Value { get; private set; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
            throw new ArgumentException("Invalid email format");

        if (normalized.Length > 254) // RFC 5321
            throw new ArgumentException("Email cannot exceed 254 characters");

        Value = normalized;
    }

    public string GetDomain()
    {
        var atIndex = Value.IndexOf('@');
        return atIndex >= 0 ? Value.Substring(atIndex + 1) : string.Empty;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is Email other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
