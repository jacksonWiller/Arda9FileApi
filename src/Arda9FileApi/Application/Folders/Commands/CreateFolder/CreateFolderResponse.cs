using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Application.Folders.Commands.CreateFolder;

public class CreateFolderResponse
{
    public FolderDto? Folder { get; set; }
    public string Message { get; set; } = string.Empty;
}