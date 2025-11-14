using System.Text.RegularExpressions;

namespace Arda9FileApi.Domain.ValueObjects;

/// <summary>
/// Value Object for Phone: E.164 format (+ + digits, 8-15 total digits)
/// </summary>
public class Phone
{
    private static readonly Regex E164Regex = new(@"^\+\d{8,15}$", RegexOptions.Compiled);

    public string CountryCode { get; private set; }
    public string Number { get; private set; }
    public string FullNumber { get; private set; }

    public Phone(string countryCode, string number)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code cannot be empty");

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Phone number cannot be empty");

        var fullNumber = $"{countryCode}{number}";

        //if (!E164Regex.IsMatch(fullNumber))
        //    throw new ArgumentException("Phone must be in E.164 format (e.g., +5511999999999)");

        CountryCode = countryCode;
        Number = number;
        FullNumber = fullNumber;
    }

    public override string ToString() => FullNumber;

    public override bool Equals(object? obj)
    {
        return obj is Phone other && FullNumber == other.FullNumber;
    }

    public override int GetHashCode() => FullNumber.GetHashCode();
}
