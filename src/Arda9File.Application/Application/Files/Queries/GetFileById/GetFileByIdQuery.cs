using Ardalis.Result;
using MediatR;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Files.Queries.GetFileById;

public class GetFileByIdQuery : IRequest<Result<FileMetadataModel>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
}