using Ardalis.Result;
using MediatR;
using System.Text.Json.Serialization;

namespace Arda9FileApi.Application.Files.Commands.UpdateFile;

public class UpdateFileCommand : IRequest<Result<UpdateFileResponse>>
{
    [JsonIgnore]
    public Guid FileId { get; set; }

    public string? FileName { get; set; }
    public Guid? FolderId { get; set; }
    public bool? IsPublic { get; set; }

    [JsonIgnore]
    public Guid TenantId { get; set; }

    [JsonIgnore]
    public Guid? UpdatedBy { get; set; }
}