using Arda9FileApi.Application.Folders.Commands.CreateFolder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arda9FileApi.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class FoldersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FoldersController> _logger;

    public FoldersController(IMediator mediator, ILogger<FoldersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova pasta
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFolder([FromBody] CreateFolderCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(CreateFolder),
                    new { id = result.Value.Folder?.Id },
                    result.Value);
            }

            return BadRequest(new { errors = result.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder");
            return StatusCode(500, new { message = "An error occurred while creating the folder" });
        }
    }
}