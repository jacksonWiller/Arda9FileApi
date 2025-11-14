using Amazon.DynamoDBv2.DataModel;

namespace Arda9FileApi.Application.DTOs;

/// <summary>
/// DTO para representar uma Image nas respostas da API e persistência no DynamoDB
/// </summary>
public class ImageDto
{
    [DynamoDBProperty]
    public Guid Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Prefix { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Url { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Width { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Height { get; set; } = string.Empty;
}
