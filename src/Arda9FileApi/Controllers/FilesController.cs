using Arda9FileApi.Api.Extensions;
using Arda9FileApi.Application.Files.Commands.UploadFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Arda9FileApi.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IMediator mediator, ILogger<FilesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("{tenantId}")]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFileAsync(Guid tenantId, [FromForm] UploadFileCommand command)
    {
        command.TenantId = tenantId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}