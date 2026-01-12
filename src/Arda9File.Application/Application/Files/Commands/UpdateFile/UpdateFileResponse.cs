using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Files.Commands.UpdateFile;

public class UpdateFileResponse
{
    public FileMetadataModel? File { get; set; }
    public string Message { get; set; } = string.Empty;
}