using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using MediatR;

namespace Arda9FileApi.Application.Folders.Queries.GetFoldersByBucket;

public class GetFoldersByBucketQuery : IRequest<Result<List<FolderDto>>>
{
    public Guid TenantId { get; set; }
    public Guid BucketId { get; set; }
}