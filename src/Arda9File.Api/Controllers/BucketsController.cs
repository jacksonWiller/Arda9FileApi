using System.Net.Mime;
using Arda9File.Application.Application.Buckets.Commands.CreateBucket;
using Arda9File.Application.Application.Buckets.Commands.DeleteBucket;
using Arda9File.Application.Application.Buckets.Queries.GetAllBuckets;
using Arda9File.Application.Application.Buckets.Queries.GetBucketById;
using Core.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arda9FileApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Authorize]
public class BucketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BucketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cria um novo bucket no S3
    /// </summary>
    /// <param name="command">Dados do bucket</param>
    /// <returns>Informações do bucket criado</returns>
    /// <response code="200">Bucket criado com sucesso</response>
    /// <response code="400">Parâmetros inválidos</response>
    /// <response code="500">Erro interno</response>
    [HttpPost()]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CreateBucketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateBucketAsync([FromBody] CreateBucketCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém todos os buckets
    /// </summary>
    /// <param name="query">Filtros de busca</param>
    /// <returns>Lista de buckets</returns>
    /// <response code="200">Buckets encontrados</response>
    /// <response code="500">Erro interno</response>
    [HttpGet()]
    [ProducesResponseType(typeof(GetAllBucketsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllBucketsAsync([FromQuery] GetAllBucketsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém um bucket pelo ID
    /// </summary>
    /// <param name="id">ID do bucket</param>
    /// <returns>Informações do bucket</returns>
    /// <response code="200">Bucket encontrado</response>
    /// <response code="404">Bucket não encontrado</response>
    /// <response code="500">Erro interno</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetBucketByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBucketByIdAsync(Guid id)
    {
        var query = new GetBucketByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Deleta um bucket
    /// </summary>
    /// <param name="bucketName">Nome do bucket</param>
    /// <param name="forceDelete">Se true, deleta todos os objetos antes de deletar o bucket</param>
    /// <returns>Status da operação</returns>
    /// <response code="204">Bucket deletado com sucesso</response>
    /// <response code="400">Parâmetros inválidos</response>
    /// <response code="404">Bucket não encontrado</response>
    /// <response code="500">Erro interno</response>
    [HttpDelete("{bucketName}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBucketAsync(string bucketName, [FromQuery] bool forceDelete = false)
    {
        var command = new DeleteBucketCommand 
        { 
            BucketName = bucketName,
            ForceDelete = forceDelete
        };
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }
}