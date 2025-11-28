using Ardalis.Result;
using MediatR;
using System.Text.Json.Serialization;

namespace Arda9FileApi.Application.Features.Folders.Commands.UpdateFolder;

public class UpdateFolderCommand : IRequest<Result<UpdateFolderResponse>>
{
    [JsonIgnore]
    public Guid FolderId { get; set; }

    public string? FolderName { get; set; }
    public bool? IsPublic { get; set; }

    [JsonIgnore]
    public Guid TenantId { get; set; }

    [JsonIgnore]
    public Guid? UpdatedBy { get; set; }
}