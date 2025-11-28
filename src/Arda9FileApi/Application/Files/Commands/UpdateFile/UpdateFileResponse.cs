using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Files.Commands.UpdateFile;

public class UpdateFileResponse
{
    public FileMetadataModel? File { get; set; }
    public string Message { get; set; } = string.Empty;
}