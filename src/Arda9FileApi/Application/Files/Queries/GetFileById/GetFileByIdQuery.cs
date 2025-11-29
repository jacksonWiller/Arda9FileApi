using Ardalis.Result;
using MediatR;
using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Files.Queries.GetFileById;

public class GetFileByIdQuery : IRequest<Result<FileMetadataModel>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
}