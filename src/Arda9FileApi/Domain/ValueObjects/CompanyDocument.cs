using System.Text.RegularExpressions;

namespace Arda9FileApi.Domain.ValueObjects;

/// <summary>
/// Value Object for Company Document (CNPJ for BR, EIN for US, etc.)
/// </summary>
public class CompanyDocument
{
    private static readonly Regex OnlyDigitsRegex = new(@"^\d+$", RegexOptions.Compiled);

    public string Value { get; private set; }
    public string Country { get; private set; }

    public CompanyDocument(string value, string country = "BR")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Company document cannot be empty");

        var normalized = Regex.Replace(value, @"[^\d]", ""); // Remove non-digits

        if (!OnlyDigitsRegex.IsMatch(normalized))
            throw new ArgumentException("Company document must contain only digits");

        // Validate by country
        //if (country.ToUpperInvariant() == "BR")
        //{
        //    if (normalized.Length != 14)
        //        throw new ArgumentException("CNPJ must have 14 digits");

        //    if (!ValidateCNPJ(normalized))
        //        throw new ArgumentException("Invalid CNPJ checksum");
        //}

        Value = normalized;
        Country = country.ToUpperInvariant();
    }

    private static bool ValidateCNPJ(string cnpj)
    {
        // Simple CNPJ validation (checksum)
        if (cnpj.All(c => c == cnpj[0])) return false; // All same digits

        int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCnpj = cnpj.Substring(0, 12);
        int sum = 0;

        for (int i = 0; i < 12; i++)
            sum += int.Parse(tempCnpj[i].ToString()) * multiplier1[i];

        int remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;

        string digit = remainder.ToString();
        tempCnpj += digit;
        sum = 0;

        for (int i = 0; i < 13; i++)
            sum += int.Parse(tempCnpj[i].ToString()) * multiplier2[i];

        remainder = sum % 11;
        remainder = remainder < 2 ? 0 : 11 - remainder;
        digit += remainder.ToString();

        return cnpj.EndsWith(digit);
    }

    public string GetMasked()
    {
        if (Country == "BR" && Value.Length == 14)
            return $"{Value.Substring(0, 2)}.{Value.Substring(2, 3)}.{Value.Substring(5, 3)}/{Value.Substring(8, 4)}-{Value.Substring(12, 2)}";

        return Value;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        return obj is CompanyDocument other && Value == other.Value && Country == other.Country;
    }

    public override int GetHashCode() => HashCode.Combine(Value, Country);
}
