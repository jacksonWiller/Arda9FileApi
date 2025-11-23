namespace Arda9FileApi.Application.Files.Commands.RestoreFile;

public class RestoreFileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
    public DateTime RestoredAt { get; set; }
}
