using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using MediatR;

namespace Arda9FileApi.Application.Files.Queries.GetFilesByBucket;

public class GetFilesByBucketQuery : IRequest<Result<List<FileMetadataDto>>>
{
    public Guid TenantId { get; set; }
    public string BucketName { get; set; } = string.Empty;
}