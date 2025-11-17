using Amazon.DynamoDBv2.DataModel;

namespace Arda9FileApi.Application.DTOs;

[DynamoDBTable("arda9-file-v1")]
public class FolderDto
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }

    [DynamoDBProperty("FolderName")]
    public string FolderName { get; set; } = string.Empty;

    [DynamoDBProperty("BucketId")]
    public Guid BucketId { get; set; }

    [DynamoDBProperty("Path")]
    public string Path { get; set; } = string.Empty;

    [DynamoDBProperty("ParentFolderId")]
    public Guid? ParentFolderId { get; set; }

    [DynamoDBProperty("CompanyId")]
    public Guid CompanyId { get; set; }

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
}