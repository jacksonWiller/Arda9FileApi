using Ardalis.Result;
using MediatR;
using System.Text.Json.Serialization;

namespace Arda9File.Application.Application.Folders.Commands.CreateFolder;

public class CreateFolderCommand : IRequest<Result<CreateFolderResponse>>
{
    public string FolderName { get; set; } = string.Empty;
    public Guid BucketId { get; set; }
    public Guid? ParentFolderId { get; set; }
    public bool IsPublic { get; set; } = false;    
}