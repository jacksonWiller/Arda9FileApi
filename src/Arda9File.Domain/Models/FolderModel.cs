using Amazon.DynamoDBv2.DataModel;
using Arda9FileApi.Core.Infra;

namespace Arda9File.Domain.Models;

/// <summary>
/// DTO for Folder with DynamoDB single table design
/// PK: FOLDER#{FolderId}, SK: METADATA
/// GSI1: BUCKET#{BucketId} -> Lista pastas por bucket
/// GSI3: COMPANY#{CompanyId} -> Lista pastas por empresa
/// </summary>
[DynamoDBTable("arda9-file-v3")]
public class FolderModel : DynamoSingleTableEntity
{
    [DynamoDBIgnore]
    public Guid Id { get; set; }

    [DynamoDBProperty("FolderId")]
    public string FolderId
    {
        get => Id.ToString();
        set => Id = Guid.Parse(value);
    }

    [DynamoDBProperty("FolderName")]
    public string FolderName { get; set; } = string.Empty;

    [DynamoDBProperty("BucketId")]
    public Guid BucketId { get; set; }

    [DynamoDBProperty("Path")]
    public string Path { get; set; } = string.Empty;

    [DynamoDBProperty("ParentFolderId")]
    public Guid? ParentFolderId { get; set; }

    [DynamoDBProperty("CompanyId")]
    public Guid TenantId { get; set; }

    [DynamoDBProperty("CreatedBy")]
    public Guid? CreatedBy { get; set; }

    [DynamoDBProperty("IsPublic")]
    public bool IsPublic { get; set; }

    [DynamoDBProperty("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [DynamoDBProperty("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [DynamoDBProperty("IsDeleted")]
    public bool IsDeleted { get; set; }

    // GSI1: Para listar folders por Bucket
    [DynamoDBGlobalSecondaryIndexHashKey("GSI1-Index", AttributeName = "GSI1PK")]
    public string GSI1PK { get; set; } = string.Empty; // BUCKET#{BucketId}

    [DynamoDBGlobalSecondaryIndexRangeKey("GSI1-Index", AttributeName = "GSI1SK")]
    public string GSI1SK { get; set; } = string.Empty; // FOLDER#{FolderId} ou CreatedAt para ordenação

    // GSI3: Para listar folders por Company
    [DynamoDBGlobalSecondaryIndexHashKey("GSI3-Index", AttributeName = "GSI3PK")]
    public string GSI3PK { get; set; } = string.Empty; // COMPANY#{CompanyId}
}