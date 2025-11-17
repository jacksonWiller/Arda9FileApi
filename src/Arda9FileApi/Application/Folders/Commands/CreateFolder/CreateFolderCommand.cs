using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Folders.Commands.CreateFolder;

public class CreateFolderCommand : IRequest<Result<CreateFolderResponse>>
{
    public string FolderName { get; set; } = string.Empty;
    public Guid BucketId { get; set; }
    public string? Path { get; set; }
    public Guid? ParentFolderId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? CreatedBy { get; set; }
    public bool IsPublic { get; set; } = false;
}