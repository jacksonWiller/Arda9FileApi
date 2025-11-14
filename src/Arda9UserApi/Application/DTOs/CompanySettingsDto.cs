using Amazon.DynamoDBv2.DataModel;

namespace Arda9UserApi.Application.DTOs;

public class CompanySettingsDto
{
    [DynamoDBProperty]
    public bool SelfRegister { get; set; } = false;

    [DynamoDBProperty]
    public bool MfaRequired { get; set; } = false;

    [DynamoDBProperty]
    public List<string> DomainsAllowed { get; set; } = [];
}
