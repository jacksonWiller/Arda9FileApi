using Amazon.DynamoDBv2.DataModel;

namespace Arda9FileApi.Application.DTOs;

/// <summary>
/// Base class for all entities in the single table design
/// </summary>
public abstract class DynamoSingleTableEntity
{
    [DynamoDBHashKey("PK")]
    public string PK { get; set; } = string.Empty;

    [DynamoDBRangeKey("SK")]
    public string SK { get; set; } = string.Empty;

    [DynamoDBProperty("EntityType")]
    public string EntityType { get; set; } = string.Empty;
}