using Amazon.DynamoDBv2.DataModel;

namespace Arda9UserApi.Application.DTOs;

public class AddressDto
{
    [DynamoDBProperty]
    public string Street { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Number { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string? Complement { get; set; }

    [DynamoDBProperty]
    public string? District { get; set; }

    [DynamoDBProperty]
    public string City { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string State { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string PostalCode { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Country { get; set; } = string.Empty;
}
