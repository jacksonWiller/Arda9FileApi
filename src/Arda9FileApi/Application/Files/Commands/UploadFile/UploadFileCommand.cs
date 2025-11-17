using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Arda9FileApi.Application.Files.Commands.UploadFile;

public class UploadFileCommand : IRequest<Result<UploadFileResponse>>
{
    public IFormFile File { get; set; } = null!;
    public string BucketName { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public Guid? SubCompanyId { get; set; }
    public Guid? ParentFolder { get; set; }
    public Guid? UploadedBy { get; set; }
    public bool IsPublic { get; set; } = false;
}