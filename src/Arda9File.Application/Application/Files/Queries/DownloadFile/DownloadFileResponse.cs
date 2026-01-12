namespace Arda9File.Application.Application.Files.Queries.DownloadFile;

public class DownloadFileResponse
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}