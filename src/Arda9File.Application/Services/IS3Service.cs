namespace Arda9File.Application.Services;

public interface IS3Service
{
    Task<bool> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType, bool isPublic = false, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadFileAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<bool> DeleteAllObjectsAsync(string bucketName, CancellationToken cancellationToken = default);
    Task<bool> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default);
    Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default);
    Task<string> GetPublicUrlAsync(string bucketName, string key);
    Task<bool> SetObjectAclAsync(string bucketName, string key, bool isPublic, CancellationToken cancellationToken = default);
    Task<bool> SetBucketPublicAccessAsync(string bucketName, CancellationToken cancellationToken = default);
    Task<bool> CreatePublicBucketAsync(string bucketName, CancellationToken cancellationToken = default);
    Task<bool> CreateBucketAsync(string bucketName, CancellationToken cancellationToken = default);
    Task<bool> SetBucketPolicyForPublicReadAsync(string bucketName, CancellationToken cancellationToken = default);
    string BuildS3Key(string? folder, Guid fileId, string fileName);
    string BuildS3Key(string? folder, string fileName);
    string SanitizeFileName(string fileName);
}