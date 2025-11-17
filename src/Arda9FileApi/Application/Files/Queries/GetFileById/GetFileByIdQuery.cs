using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using MediatR;

namespace Arda9FileApi.Application.Files.Queries.GetFileById;

public class GetFileByIdQuery : IRequest<Result<FileMetadataDto>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
}