using Amazon.DynamoDBv2.DataModel;

namespace Arda9File.Domain.Models;

public class PhoneModel
{
    [DynamoDBProperty]
    public string CountryCode { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Number { get; set; } = string.Empty;
}
