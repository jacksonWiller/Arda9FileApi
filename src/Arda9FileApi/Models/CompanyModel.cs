using Amazon.DynamoDBv2.DataModel;

namespace Arda9FileApi.Models;

/// <summary>
/// DTO for Company with DynamoDB annotations
/// PK: COMPANY#<companyId>, SK: METADATA
/// </summary>
[DynamoDBTable("arda-user-v1")]
public class CompanyModel
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Slug { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string? Document { get; set; }

    [DynamoDBProperty]
    public string? DocumentCountry { get; set; }

    [DynamoDBProperty]
    public string? Email { get; set; }

    [DynamoDBProperty]
    public PhoneModel? Phone { get; set; }

    [DynamoDBProperty]
    public AddressModel? Address { get; set; }

    [DynamoDBProperty]
    public List<string> Tags { get; set; } = [];

    [DynamoDBProperty]
    public string Status { get; set; } = CompanyStatus.ACTIVE.ToString();

    [DynamoDBProperty]
    public CompanySettingsModel Settings { get; set; } = new();

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }

    [DynamoDBProperty]
    public DateTime UpdatedAt { get; set; }

    [DynamoDBProperty]
    public Guid? CreatedBy { get; set; }

    [DynamoDBProperty]
    public Guid? UpdatedBy { get; set; }
}
