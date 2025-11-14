using System.Text.RegularExpressions;

namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Value Object for Company Name: 2-120 chars, trimmed, no control chars
/// </summary>
public class CompanyName
{
    private static readonly Regex ControlCharsRegex = new(@"[\x00-\x1F\x7F]", RegexOptions.Compiled);

    public string Value { get; private set; }

    public CompanyName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Company name cannot be empty");

        var normalized = NormalizeWhitespace(value.Trim());

        if (normalized.Length < 2 || normalized.Length > 120)
            throw new ArgumentException("Company name must be between 2 and 120 characters");

        if (ControlCharsRegex.IsMatch(normalized))
            throw new ArgumentException("Company name cannot contain control characters");

        Value = normalized;
    }

    private static string NormalizeWhitespace(string input)
    {
        return Regex.Replace(input, @"\s+", " ");
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is CompanyName other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
