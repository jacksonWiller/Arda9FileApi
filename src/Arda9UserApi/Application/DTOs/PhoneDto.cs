using Amazon.DynamoDBv2.DataModel;

namespace Arda9UserApi.Application.DTOs;

public class PhoneDto
{
    [DynamoDBProperty]
    public string CountryCode { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Number { get; set; } = string.Empty;
}
