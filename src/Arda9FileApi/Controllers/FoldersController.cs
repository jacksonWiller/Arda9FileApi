using Arda9FileApi.Api.Extensions;
using Arda9FileApi.Application.Folders.Commands.CreateFolder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arda9FileApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    [HttpPost("{tenantId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFolder(Guid tenantId, [FromBody] CreateFolderCommand command)
    {
        command.TenantId = tenantId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}