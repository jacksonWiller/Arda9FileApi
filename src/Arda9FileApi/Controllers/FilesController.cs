using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arda9FileApi.Application.Files.Commands.UploadFile;

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

    [HttpPost]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFileAsync([FromForm] UploadFileCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return result.Status switch
            {
                Ardalis.Result.ResultStatus.NotFound => NotFound(result.Errors),
                Ardalis.Result.ResultStatus.Invalid => BadRequest(result.ValidationErrors),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result.Errors)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload do arquivo");
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro interno ao processar a requisição");
        }
    }
}