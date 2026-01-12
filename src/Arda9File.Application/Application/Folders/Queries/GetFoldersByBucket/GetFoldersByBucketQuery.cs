using Ardalis.Result;
using MediatR;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Folders.Queries.GetFoldersByBucket;

public class GetFoldersByBucketQuery : IRequest<Result<List<FolderModel>>>
{
    public Guid TenantId { get; set; }
    public Guid BucketId { get; set; }
}