using Arda9FileApi.Application.Folders.Commands.DeleteFolder;
using Arda9FileApi.Application.Folders.Queries.GetFolderById;
using Arda9FileApi.Application.Folders.Queries.GetFoldersByBucket;
using Arda9FileApi.Application.Folders.Queries.GetFoldersByParent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Arda9FileApi.Application.Folders.Commands.CreateFolder;
using Arda9FileApi.Application.Folders.Commands.MoveFolder;
using Arda9FileApi.Application.Folders.Commands.UpdateFolder;
using Arda9FileApi.Application.Folders.Queries.GetFolders;
using Core.Api.Extensions;

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
    [HttpGet("{tenantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFolders(Guid tenantId, [FromQuery] GetFoldersQuery query)
    {
        query.TenantId = tenantId;
        var result = await _mediator.Send(query);
        return result.ToActionResult();
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
        
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result);
        }
        
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém uma pasta por ID
    /// </summary>
    [HttpGet("{tenantId}/{folderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFolderById(Guid tenantId, Guid folderId)
    {
        var query = new GetFolderByIdQuery { TenantId = tenantId, FolderId = folderId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém todas as pastas de um bucket
    /// </summary>
    [HttpGet("{tenantId}/bucket/{bucketId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFoldersByBucket(Guid tenantId, Guid bucketId)
    {
        var query = new GetFoldersByBucketQuery { TenantId = tenantId, BucketId = bucketId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém as subpastas de uma pasta pai
    /// </summary>
    [HttpGet("{tenantId}/parent/{parentFolderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFoldersByParent(Guid tenantId, Guid parentFolderId)
    {
        var query = new GetFoldersByParentQuery { TenantId = tenantId, ParentFolderId = parentFolderId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Atualiza uma pasta
    /// </summary>
    [HttpPatch("{tenantId}/{folderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateFolder(Guid tenantId, Guid folderId, [FromBody] UpdateFolderCommand command)
    {
        command.TenantId = tenantId;
        command.FolderId = folderId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Exclui uma pasta (soft delete)
    /// </summary>
    [HttpDelete("{tenantId}/{folderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFolder(
        Guid tenantId, 
        Guid folderId,
        [FromQuery] bool recursive = false,
        [FromQuery] bool permanent = false)
    {
        var command = new DeleteFolderCommand { TenantId = tenantId, FolderId = folderId };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Move pasta para outro local
    /// </summary>
    [HttpPost("{tenantId}/{folderId}/move")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MoveFolder(Guid tenantId, Guid folderId, [FromBody] MoveFolderCommand command)
    {
        command.TenantId = tenantId;
        command.FolderId = folderId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}