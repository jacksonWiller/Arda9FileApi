using Amazon.DynamoDBv2.DataModel;

namespace Arda9File.Domain.Models;

public class CompanySettingsModel
{
    [DynamoDBProperty]
    public bool SelfRegister { get; set; } = false;

    [DynamoDBProperty]
    public bool MfaRequired { get; set; } = false;

    [DynamoDBProperty]
    public List<string> DomainsAllowed { get; set; } = [];
}
