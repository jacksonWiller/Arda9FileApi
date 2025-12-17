using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Arda9FileApi.Application.Files.Commands.UploadFile;

public class UploadFileCommand : IRequest<Result<UploadFileResponse>>
{
    public IFormFile File { get; set; } = null!;
    public Guid BucketId { get; set; }
    public Guid? FolderId { get; set; }
    public bool IsPublic { get; set; } = false;

    [JsonIgnore]
    public Guid TenantId { get; set; }

    [JsonIgnore]
    public Guid? UpdatedBy { get; set; }
}