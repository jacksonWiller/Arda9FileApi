using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Files.Queries.DownloadFile;

public class DownloadFileQuery : IRequest<Result<DownloadFileResponse>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
}