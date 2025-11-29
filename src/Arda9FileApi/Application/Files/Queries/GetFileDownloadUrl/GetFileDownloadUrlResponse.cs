namespace Arda9FileApi.Application.Files.Queries.GetFileDownloadUrl;

public class GetFileDownloadUrlResponse
{
    public string Url { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Filename { get; set; } = string.Empty;
    public long Size { get; set; }
}
