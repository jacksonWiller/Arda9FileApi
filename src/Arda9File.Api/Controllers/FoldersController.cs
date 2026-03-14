using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Api.Extensions;
using Arda9File.Application.Application.Folders.Commands.CreateFolder;
using Arda9File.Application.Application.Folders.Commands.MoveFolder;
using Arda9File.Application.Application.Folders.Commands.UpdateFolder;
using Arda9File.Application.Application.Folders.Queries.GetFolders;
using Arda9File.Application.Application.Folders.Queries.GetFolderById;
using Arda9File.Application.Application.Folders.Queries.GetFoldersByBucket;
using Arda9File.Application.Application.Folders.Queries.GetFoldersByParent;
using Arda9File.Application.Application.Folders.Commands.DeleteFolder;

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
    /// Lista todas as pastas do usuário com estrutura hierárquica
    /// </summary>
    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFolders([FromQuery] GetFoldersQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Cria uma nova pasta
    /// </summary>
    [HttpPost()]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFolder([FromBody] CreateFolderCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result);
        }
        
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém uma pasta por ID
    /// </summary>
    [HttpGet("{folderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFolderById(Guid folderId)
    {
        var query = new GetFolderByIdQuery { FolderId = folderId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém todas as pastas de um bucket
    /// </summary>
    [HttpGet("bucket/{bucketId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFoldersByBucket(Guid tenantId, Guid bucketId)
    {
        var query = new GetFoldersByBucketQuery { BucketId = bucketId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém as subpastas de uma pasta pai
    /// </summary>
    [HttpGet("parent/{parentFolderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFoldersByParent(Guid tenantId, Guid parentFolderId)
    {
        var query = new GetFoldersByParentQuery { ParentFolderId = parentFolderId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Atualiza uma pasta
    /// </summary>
    [HttpPatch("{folderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateFolder(Guid folderId, [FromBody] UpdateFolderCommand command)
    {
        command.FolderId = folderId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Exclui uma pasta (soft delete)
    /// </summary>
    [HttpDelete("{folderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFolder(
        Guid tenantId, 
        Guid folderId,
        [FromQuery] bool recursive = false,
        [FromQuery] bool permanent = false)
    {
        var command = new DeleteFolderCommand { FolderId = folderId };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Move pasta para outro local
    /// </summary>
    [HttpPost("s{folderId}/move")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MoveFolder(Guid folderId, [FromBody] MoveFolderCommand command)
    {
        command.FolderId = folderId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}