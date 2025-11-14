namespace Arda9FileApi.Domain.ValueObjects;

/// <summary>
/// Value Object for Address
/// </summary>
public class Address
{
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string? Complement { get; private set; }
    public string? District { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; } // ISO-3166-1 alpha-2

    public Address(
        string street,
        string number,
        string city,
        string state,
        string postalCode,
        string country,
        string? complement = null,
        string? district = null)
    {
        if (string.IsNullOrWhiteSpace(street) || street.Length > 120)
            throw new ArgumentException("Street must be between 1 and 120 characters");

        if (string.IsNullOrWhiteSpace(number) || number.Length > 20)
            throw new ArgumentException("Number must be between 1 and 20 characters");

        if (complement != null && complement.Length > 60)
            throw new ArgumentException("Complement cannot exceed 60 characters");

        if (district != null && district.Length > 60)
            throw new ArgumentException("District cannot exceed 60 characters");

        if (string.IsNullOrWhiteSpace(city) || city.Length > 80)
            throw new ArgumentException("City must be between 1 and 80 characters");

        if (string.IsNullOrWhiteSpace(state) || state.Length > 40)
            throw new ArgumentException("State must be between 1 and 40 characters");

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty");

        if (string.IsNullOrWhiteSpace(country) || country.Length != 2)
            throw new ArgumentException("Country must be ISO-3166-1 alpha-2 code (2 characters)");

        Street = street.Trim();
        Number = number.Trim();
        Complement = complement?.Trim();
        District = district?.Trim();
        City = city.Trim();
        State = state.Trim();
        PostalCode = postalCode.Trim();
        Country = country.ToUpperInvariant();
    }

    public override string ToString() =>
        $"{Street}, {Number}{(Complement != null ? $" - {Complement}" : "")}, {City}/{State} - {PostalCode}, {Country}";
}
