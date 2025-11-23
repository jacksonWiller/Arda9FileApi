using Amazon.DynamoDBv2.DataModel;

namespace Arda9FileApi.Application.DTOs;

/// <summary>
/// DTO for File Metadata with DynamoDB single table design
/// PK: FILE#{FileId}, SK: METADATA
/// GSI1: BUCKET#{BucketId} -> Lista arquivos por bucket
/// GSI2: FOLDER#{FolderId} -> Lista arquivos por pasta
/// GSI3: COMPANY#{CompanyId} -> Lista arquivos por empresa
/// </summary>
[DynamoDBTable("arda9-file-v3")]
public class FileMetadataDto : DynamoSingleTableEntity
{
    [DynamoDBIgnore]
    public Guid FileId { get; set; }

    [DynamoDBProperty("FileId")]
    public string FileIdString
    {
        get => FileId.ToString();
        set => FileId = Guid.Parse(value);
    }

    [DynamoDBProperty("FileName")]
    public string FileName { get; set; } = string.Empty;

    [DynamoDBProperty("BucketName")]
    public string BucketName { get; set; } = string.Empty;

    [DynamoDBProperty("BucketId")]
    public Guid BucketId { get; set; }

    [DynamoDBProperty("S3Key")]
    public string S3Key { get; set; } = string.Empty;

    [DynamoDBProperty("ContentType")]
    public string ContentType { get; set; } = string.Empty;

    [DynamoDBProperty("Size")]
    public long Size { get; set; }

    [DynamoDBProperty("Folder")]
    public string? Folder { get; set; }

    [DynamoDBProperty("FolderId")]
    public Guid? FolderId { get; set; }

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

    // GSI1: Para listar arquivos por Bucket
    [DynamoDBGlobalSecondaryIndexHashKey("GSI1-Index", AttributeName = "GSI1PK")]
    public string GSI1PK { get; set; } = string.Empty; // BUCKET#{BucketId}

    [DynamoDBGlobalSecondaryIndexRangeKey("GSI1-Index", AttributeName = "GSI1SK")]
    public string GSI1SK { get; set; } = string.Empty; // FILE#{FileId} ou CreatedAt para ordenação

    // GSI2: Para listar arquivos por Folder
    [DynamoDBGlobalSecondaryIndexHashKey("GSI2-Index", AttributeName = "GSI2PK")]
    public string GSI2PK { get; set; } = string.Empty; // FOLDER#{FolderId}

    [DynamoDBGlobalSecondaryIndexRangeKey("GSI2-Index", AttributeName = "GSI2SK")]
    public string GSI2SK { get; set; } = string.Empty; // FILE#{FileId} ou CreatedAt para ordenação

    // GSI3: Para listar arquivos por Company
    [DynamoDBGlobalSecondaryIndexHashKey("GSI3-Index", AttributeName = "GSI3PK")]
    public string GSI3PK { get; set; } = string.Empty; // COMPANY#{CompanyId}
}