using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Application.Files.Commands.UpdateFile;

public class UpdateFileResponse
{
    public FileMetadataDto? File { get; set; }
    public string Message { get; set; } = string.Empty;
}