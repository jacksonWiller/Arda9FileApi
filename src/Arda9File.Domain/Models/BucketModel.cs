using Amazon.DynamoDBv2.DataModel;
using Arda9FileApi.Core.Infra;

namespace Arda9FileApi.Models;

/// <summary>
/// DTO for Bucket with DynamoDB single table design
/// PK: BUCKET#{BucketId}, SK: METADATA
/// GSI3: COMPANY#{CompanyId} -> Lista buckets por empresa
/// </summary>
[DynamoDBTable("arda9-file-v3")]
public class BucketModel : DynamoSingleTableEntity
{
    [DynamoDBIgnore]
    public Guid Id { get; set; }

    [DynamoDBProperty("BucketId")]
    public string BucketId
    {
        get => Id.ToString();
        set => Id = Guid.Parse(value);
    }

    [DynamoDBProperty]
    public string BucketName { get; set; } = string.Empty;

    [DynamoDBProperty]
    public Guid CompanyId { get; set; }

    [DynamoDBProperty]
    public Guid? SubCompanyId { get; set; }

    [DynamoDBProperty]
    public string Region { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Status { get; set; } = string.Empty;

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }

    [DynamoDBProperty]
    public DateTime UpdatedAt { get; set; }

    [DynamoDBProperty]
    public string? CreatedBy { get; set; }

    [DynamoDBProperty]
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Additional metadata from S3 - not persisted in DynamoDB
    /// </summary>
    [DynamoDBIgnore]
    public long? Size { get; set; }

    /// <summary>
    /// Files list for response DTOs - not persisted in DynamoDB
    /// </summary>
    [DynamoDBIgnore]
    public List<FileMetadataModel> Files { get; set; } = new();

    /// <summary>
    /// Folders list for response DTOs - not persisted in DynamoDB
    /// </summary>
    [DynamoDBIgnore]
    public List<FolderModel> Folders { get; set; } = new();

    // GSI3: Para listar buckets por Company
    [DynamoDBGlobalSecondaryIndexHashKey("GSI3-Index", AttributeName = "GSI3PK")]
    public string GSI3PK { get; set; } = string.Empty; // COMPANY#{CompanyId}
}