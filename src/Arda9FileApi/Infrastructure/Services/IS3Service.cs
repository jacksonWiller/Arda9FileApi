namespace Arda9FileApi.Infrastructure.Services;

public interface IS3Service
{
    Task<bool> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadFileAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<bool> DeleteAllObjectsAsync(string bucketName, CancellationToken cancellationToken = default);
    Task<bool> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default);
    Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default);
}