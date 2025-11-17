using Amazon.DynamoDBv2.DataModel;

namespace Arda9FileApi.Application.DTOs;

/// <summary>
/// DTO for Bucket with DynamoDB annotations
/// Combines data from S3 and DynamoDB
/// </summary>
[DynamoDBTable("arda9-file-v1")]
public class BucketDto
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }
    
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
    
    /// <summary>
    /// Creation date from S3 or DynamoDB
    /// </summary>
    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; }
    
    [DynamoDBProperty]
    public DateTime UpdatedAt { get; set; }
    
    [DynamoDBProperty]
    public Guid? CreatedBy { get; set; }
    
    [DynamoDBProperty]
    public Guid? UpdatedBy { get; set; }
    
    /// <summary>
    /// Additional metadata from S3
    /// </summary>
    [DynamoDBIgnore]
    public long? Size { get; set; }

    [DynamoDBProperty]
    public List<FileMetadataDto> Files { get; set; } = new();

}