using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Application.Files.Commands.UploadFile;

public class UploadFileResponse
{
    public FileMetadataDto? FileMetadata { get; set; }
    public string Message { get; set; } = string.Empty;
}