namespace Arda9FileApi.Domain.ValueObjects;

/// <summary>
/// Value Object for Company Settings
/// </summary>
public class CompanySettings
{
    public bool SelfRegister { get; private set; }
    public bool MfaRequired { get; private set; }
    public List<string> DomainsAllowed { get; private set; }

    public CompanySettings(
        bool selfRegister = false,
        bool mfaRequired = false,
        List<string>? domainsAllowed = null)
    {
        SelfRegister = selfRegister;
        MfaRequired = mfaRequired;
        DomainsAllowed = ValidateAndNormalizeDomains(domainsAllowed ?? new List<string>());
    }

    private static List<string> ValidateAndNormalizeDomains(List<string> domains)
    {
        if (domains.Count > 20)
            throw new ArgumentException("Cannot have more than 20 allowed domains");

        var normalized = new List<string>();
        foreach (var domain in domains)
        {
            if (string.IsNullOrWhiteSpace(domain))
                continue;

            var lower = domain.Trim().ToLowerInvariant();

            // Basic domain validation
            if (!System.Text.RegularExpressions.Regex.IsMatch(lower, @"^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$"))
                throw new ArgumentException($"Invalid domain format: {domain}");

            if (normalized.Contains(lower))
                throw new ArgumentException($"Duplicate domain: {domain}");

            normalized.Add(lower);
        }

        return normalized;
    }

    public CompanySettings Update(bool? selfRegister = null, bool? mfaRequired = null, List<string>? domainsAllowed = null)
    {
        return new CompanySettings(
            selfRegister ?? SelfRegister,
            mfaRequired ?? MfaRequired,
            domainsAllowed ?? DomainsAllowed
        );
    }
}
