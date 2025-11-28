using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Files.Commands.UploadFile;

public class UploadFileResponse
{
    public FileMetadataModel? FileMetadata { get; set; }
    public string Message { get; set; } = string.Empty;
}