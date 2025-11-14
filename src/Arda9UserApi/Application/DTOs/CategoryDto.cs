using Amazon.DynamoDBv2.DataModel;

namespace Arda9FileApi.Application.DTOs;

/// <summary>
/// DTO para representar uma Category nas respostas da API e persistência no DynamoDB
/// </summary>
public class CategoryDto
{
    [DynamoDBProperty]
    public Guid Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Description { get; set; } = string.Empty;

    [DynamoDBProperty]
    public bool IsDeleted { get; set; } = false;
}
