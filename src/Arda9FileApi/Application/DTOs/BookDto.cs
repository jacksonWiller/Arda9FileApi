using Amazon.DynamoDBv2.DataModel;

namespace Arda9FileApi.Application.DTOs;

/// <summary>
/// DTO para representar um Book nas respostas da API e persistência no DynamoDB
/// </summary>
[DynamoDBTable("arda-user-v1")]
public class BookDto
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Description { get; set; } = string.Empty;

    [DynamoDBProperty]
    public decimal Price { get; set; }

    [DynamoDBProperty]
    public int StockQuantity { get; set; }

    [DynamoDBProperty]
    public string SKU { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Brand { get; set; } = string.Empty;

    [DynamoDBProperty]
    public List<CategoryDto> Categories { get; set; } = [];

    [DynamoDBProperty]
    public List<ImageDto> Images { get; set; } = [];

    [DynamoDBProperty]
    public List<TagDto> Tags { get; set; } = [];

    [DynamoDBProperty]
    public bool IsDeleted { get; set; } = false;

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [DynamoDBProperty]
    public DateTime? UpdatedAt { get; set; }
}
