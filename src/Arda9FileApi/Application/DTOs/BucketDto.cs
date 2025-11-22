using Amazon.DynamoDBv2.DataModel;

namespace Arda9FileApi.Application.DTOs;

/// <summary>
/// DTO for Bucket with DynamoDB single table design
/// PK: BUCKET#{BucketId}, SK: METADATA
/// GSI3: COMPANY#{CompanyId} -> Lista buckets por empresa
/// </summary>
[DynamoDBTable("arda9-file-v2")]
public class BucketDto : DynamoSingleTableEntity
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
    public Guid? CreatedBy { get; set; }

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
    public List<FileMetadataDto> Files { get; set; } = new();

    /// <summary>
    /// Folders list for response DTOs - not persisted in DynamoDB
    /// </summary>
    [DynamoDBIgnore]
    public List<FolderDto> Folders { get; set; } = new();

    // GSI3: Para listar buckets por Company
    [DynamoDBGlobalSecondaryIndexHashKey("GSI3-Index", AttributeName = "GSI3PK")]
    public string GSI3PK { get; set; } = string.Empty; // COMPANY#{CompanyId}
}