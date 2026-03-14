using Ardalis.Result;
using MediatR;
using System.Text.Json.Serialization;

namespace Arda9File.Application.Application.Folders.Commands.UpdateFolder;

public class UpdateFolderCommand : IRequest<Result<UpdateFolderResponse>>
{
    [JsonIgnore]
    public Guid FolderId { get; set; }

    public string? FolderName { get; set; }
    public bool? IsPublic { get; set; }

}