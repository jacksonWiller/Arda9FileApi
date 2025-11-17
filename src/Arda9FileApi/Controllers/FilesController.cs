using Arda9FileApi.Api.Extensions;
using Arda9FileApi.Application.Files.Commands.UploadFile;
using Arda9FileApi.Application.Files.Commands.UpdateFile;
using Arda9FileApi.Application.Files.Commands.DeleteFile;
using Arda9FileApi.Application.Files.Queries.GetFileById;
using Arda9FileApi.Application.Files.Queries.GetFilesByBucket;
using Arda9FileApi.Application.Files.Queries.GetFilesByFolder;
using Arda9FileApi.Application.Files.Queries.DownloadFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Arda9FileApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IMediator mediator, ILogger<FilesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Faz upload de um arquivo
    /// </summary>
    [HttpPost("{tenantId}")]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFileAsync(Guid tenantId, [FromForm] UploadFileCommand command)
    {
        command.TenantId = tenantId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém um arquivo por ID
    /// </summary>
    [HttpGet("{tenantId}/{fileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFileById(Guid tenantId, Guid fileId)
    {
        var query = new GetFileByIdQuery { TenantId = tenantId, FileId = fileId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém todos os arquivos de um bucket
    /// </summary>
    [HttpGet("{tenantId}/bucket/{bucketName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFilesByBucket(Guid tenantId, string bucketName)
    {
        var query = new GetFilesByBucketQuery { TenantId = tenantId, BucketName = bucketName };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém todos os arquivos de uma pasta
    /// </summary>
    [HttpGet("{tenantId}/folder/{folderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFilesByFolder(Guid tenantId, Guid folderId)
    {
        var query = new GetFilesByFolderQuery { TenantId = tenantId, FolderId = folderId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Faz download de um arquivo
    /// </summary>
    [HttpGet("{tenantId}/{fileId}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadFile(Guid tenantId, Guid fileId)
    {
        var query = new DownloadFileQuery { TenantId = tenantId, FileId = fileId };
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return result.ToActionResult();
        }

        return File(result.Value.FileStream, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>
    /// Atualiza os metadados de um arquivo
    /// </summary>
    [HttpPut("{tenantId}/{fileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateFile(Guid tenantId, Guid fileId, [FromBody] UpdateFileCommand command)
    {
        command.TenantId = tenantId;
        command.FileId = fileId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Exclui um arquivo (soft delete)
    /// </summary>
    [HttpDelete("{tenantId}/{fileId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFile(Guid tenantId, Guid fileId)
    {
        var command = new DeleteFileCommand { TenantId = tenantId, FileId = fileId };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}