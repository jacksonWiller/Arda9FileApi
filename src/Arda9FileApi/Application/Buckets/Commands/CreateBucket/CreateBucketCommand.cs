using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Buckets.Commands.CreateBucket;

public class CreateBucketCommand : IRequest<Result<CreateBucketResponse>>
{
    public string BucketName { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public Guid? SubCompanyId { get; set; }
    public string? Region { get; set; }
    public Guid? CreatedBy { get; set; }
}