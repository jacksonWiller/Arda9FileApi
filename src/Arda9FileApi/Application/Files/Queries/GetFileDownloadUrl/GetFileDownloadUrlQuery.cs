using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Files.Queries.GetFileDownloadUrl;

public class GetFileDownloadUrlQuery : IRequest<Result<GetFileDownloadUrlResponse>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
    public int? Version { get; set; }
}
