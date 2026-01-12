using Ardalis.Result;
using MediatR;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Files.Queries.GetRootFiles;

public class GetRootFilesQuery : IRequest<Result<List<FileMetadataModel>>>
{
    public Guid TenantId { get; set; }
    public Guid BucketId { get; set; }
}
