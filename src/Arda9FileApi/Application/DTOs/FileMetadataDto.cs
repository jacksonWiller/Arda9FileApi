using Amazon.DynamoDBv2.DataModel;

namespace Arda9FileApi.Application.DTOs;

[DynamoDBTable("arda9-file-v1")]
public class FileMetadataDto
{
    [DynamoDBHashKey("PK")]
    public string PK { get; set; } = string.Empty; // FILE#{FileId}

    [DynamoDBRangeKey("SK")]
    public string SK { get; set; } = string.Empty; // METADATA

    [DynamoDBProperty("FileId")]
    public Guid FileId { get; set; }

    [DynamoDBProperty("FileName")]
    public string FileName { get; set; } = string.Empty;

    [DynamoDBProperty("BucketName")]
    public string BucketName { get; set; } = string.Empty;

    [DynamoDBProperty("S3Key")]
    public string S3Key { get; set; } = string.Empty;

    [DynamoDBProperty("ContentType")]
    public string ContentType { get; set; } = string.Empty;

    [DynamoDBProperty("Size")]
    public long Size { get; set; }

    [DynamoDBProperty("Folder")]
    public string? Folder { get; set; }

    [DynamoDBProperty("CompanyId")]
    public Guid CompanyId { get; set; }

    [DynamoDBProperty("SubCompanyId")]
    public Guid? SubCompanyId { get; set; }

    [DynamoDBProperty("UploadedBy")]
    public Guid? UploadedBy { get; set; }

    [DynamoDBProperty("IsPublic")]
    public bool IsPublic { get; set; }

    [DynamoDBProperty("PublicUrl")]
    public string? PublicUrl { get; set; }

    [DynamoDBProperty("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [DynamoDBProperty("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [DynamoDBProperty("IsDeleted")]
    public bool IsDeleted { get; set; }
}