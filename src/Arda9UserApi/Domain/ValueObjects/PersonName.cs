using System.Text.RegularExpressions;

namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Value Object for Person Name: 2-100 chars, trimmed, no control chars
/// </summary>
public class PersonName
{
    private static readonly Regex ControlCharsRegex = new(@"[\x00-\x1F\x7F]", RegexOptions.Compiled);

    public string Value { get; private set; }

    public PersonName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Person name cannot be null or whitespace");

        var normalized = NormalizeWhitespace(value.Trim());

        if (normalized.Length < 2 || normalized.Length > 100)
            throw new ArgumentException("Person name must be between 2 and 100 characters");

        if (ControlCharsRegex.IsMatch(normalized))
            throw new ArgumentException("Person name cannot contain control characters");

        Value = normalized;
    }

    private static string NormalizeWhitespace(string input)
    {
        return Regex.Replace(input, @"\s+", " ");
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        if (obj is PersonName other)
            return Value.Equals(other.Value, StringComparison.Ordinal);
        return false;
    }

    public override int GetHashCode() => Value.GetHashCode();
}