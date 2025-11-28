using Ardalis.Result;
using MediatR;
using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Features.Folders.Queries.GetFoldersByBucket;

public class GetFoldersByBucketQuery : IRequest<Result<List<FolderModel>>>
{
    public Guid TenantId { get; set; }
    public Guid BucketId { get; set; }
}