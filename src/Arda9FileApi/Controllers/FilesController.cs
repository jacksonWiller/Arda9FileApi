using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Arda9FileApi.Core.Api.Extensions;
using Arda9FileApi.Application.Files.Commands.DuplicateFile;
using Arda9FileApi.Application.Files.Commands.MoveFile;
using Arda9FileApi.Application.Files.Commands.UpdateFile;
using Arda9FileApi.Application.Files.Commands.UploadFile;
using Arda9FileApi.Application.Files.Queries.GetFiles;
using Arda9FileApi.Application.Files.Queries.GetFileById;
using Arda9FileApi.Application.Files.Queries.GetFilesByBucket;
using Arda9FileApi.Application.Files.Queries.GetRootFiles;
using Arda9FileApi.Application.Files.Queries.GetFilesByFolder;
using Arda9FileApi.Application.Files.Queries.DownloadFile;
using Arda9FileApi.Application.Files.Queries.GetFileDownloadUrl;
using Arda9FileApi.Application.Files.Commands.DeleteFile;
using Arda9FileApi.Application.Files.Commands.RestoreFile;

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
    /// Lista arquivos do usuário com filtros e paginação
    /// </summary>
    [HttpGet("{tenantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFiles(
        Guid tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string? order = "desc",
        [FromQuery] Guid? folderId = null,
        [FromQuery] string? type = null,
        [FromQuery] string? extension = null,
        [FromQuery] bool? isFavorite = null,
        [FromQuery] bool? isShared = null,
        [FromQuery] string? tags = null,
        [FromQuery] string? search = null,
        [FromQuery] long? minSize = null,
        [FromQuery] long? maxSize = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new GetFilesQuery
        {
            TenantId = tenantId,
            Page = page,
            Limit = limit,
            SortBy = sortBy,
            Order = order,
            FolderId = folderId,
            Type = type,
            Extension = extension,
            IsFavorite = isFavorite,
            IsShared = isShared,
            Tags = tags,
            Search = search,
            MinSize = minSize,
            MaxSize = maxSize,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query);
        return result.ToActionResult();
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
        
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result);
        }
        
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
    [HttpGet("{tenantId}/bucket/{bucketId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFilesByBucket(Guid tenantId, Guid bucketId)
    {
        var query = new GetFilesByBucketQuery { TenantId = tenantId, BucketId = bucketId };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém todos os arquivos da pasta raiz (sem pasta pai)
    /// </summary>
    [HttpGet("{tenantId}/bucket/{bucketId}/root")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRootFiles(Guid tenantId, Guid bucketId)
    {
        var query = new GetRootFilesQuery { TenantId = tenantId, BucketId = bucketId };
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
    /// Retorna URL assinada para download
    /// </summary>
    [HttpGet("{tenantId}/{fileId}/download-url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDownloadUrl(Guid tenantId, Guid fileId, [FromQuery] int? version = null)
    {
        var query = new GetFileDownloadUrlQuery 
        { 
            TenantId = tenantId, 
            FileId = fileId,
            Version = version
        };
        
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Atualiza os metadados de um arquivo
    /// </summary>
    [HttpPatch("{tenantId}/{fileId}")]
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
    /// Move arquivo para lixeira (soft delete)
    /// </summary>
    [HttpDelete("{tenantId}/{fileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFile(Guid tenantId, Guid fileId, [FromQuery] bool permanent = false)
    {
        var command = new DeleteFileCommand { TenantId = tenantId, FileId = fileId };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Restaura arquivo da lixeira
    /// </summary>
    [HttpPost("{tenantId}/{fileId}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RestoreFile(Guid tenantId, Guid fileId)
    {
        var command = new RestoreFileCommand { TenantId = tenantId, FileId = fileId };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Cria uma cópia do arquivo
    /// </summary>
    [HttpPost("{tenantId}/{fileId}/duplicate")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DuplicateFile(Guid tenantId, Guid fileId, [FromBody] DuplicateFileCommand command)
    {
        command.TenantId = tenantId;
        command.FileId = fileId;
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result);
        }
        
        return result.ToActionResult();
    }

    /// <summary>
    /// Move arquivo para outra pasta
    /// </summary>
    [HttpPost("{tenantId}/{fileId}/move")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MoveFile(Guid tenantId, Guid fileId, [FromBody] MoveFileCommand command)
    {
        command.TenantId = tenantId;
        command.FileId = fileId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}