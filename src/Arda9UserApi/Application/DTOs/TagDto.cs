using Amazon.DynamoDBv2.DataModel;

namespace Arda9UserApi.Application.DTOs;

/// <summary>
/// DTO para representar uma Tag nas respostas da API e persistência no DynamoDB
/// </summary>
public class TagDto
{
    [DynamoDBProperty]
    public Guid Id { get; set; }

    [DynamoDBProperty]
    public string Name { get; set; } = string.Empty;
}
