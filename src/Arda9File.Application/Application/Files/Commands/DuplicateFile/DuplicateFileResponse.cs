namespace Arda9File.Application.Application.Files.Commands.DuplicateFile;

public class DuplicateFileResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
}
