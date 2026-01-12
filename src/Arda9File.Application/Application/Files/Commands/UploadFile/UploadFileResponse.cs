using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Files.Commands.UploadFile;

public class UploadFileResponse
{
    public FileMetadataModel? FileMetadata { get; set; }
    public string Message { get; set; } = string.Empty;
}