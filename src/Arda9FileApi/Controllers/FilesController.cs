//using System.Net.Mime;
//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using Arda9FileApi.Application.Files.Commands;
//using Arda9FileApi.Application.Files.Queries;
//using Arda9UserApi.Api.Extensions;

//namespace Arda9FileApi.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class FilesController : ControllerBase
//{
//    private readonly IMediator _mediator;

//    public FilesController(IMediator mediator)
//    {
//        _mediator = mediator;
//    }

//    [HttpPost]
//    [Consumes(MediaTypeNames.Multipart.FormData)]
//    [Produces(MediaTypeNames.Application.Json)]
//    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> UploadFileAsync([FromForm] UploadFileCommand command)
//    {
//        var result = await _mediator.Send(command);
//        return result.ToActionResult();
//    }

//    [HttpGet]
//    [ProducesResponseType(typeof(IEnumerable<S3ObjectDto>), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> GetAllFilesAsync([FromQuery] GetAllFilesQuery query)
//    {
//        var result = await _mediator.Send(query);
//        return result.ToActionResult();
//    }

//    [HttpGet("{key}")]
//    [ProducesResponseType(typeof(GetFileByKeyResponse), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> GetFileByKeyAsync(string key, [FromQuery] string bucketName)
//    {
//        var query = new GetFileByKeyQuery { Key = key, BucketName = bucketName };
//        var result = await _mediator.Send(query);
//        return result.ToActionResult();
//    }

//    [HttpDelete("{key}")]
//    [ProducesResponseType(StatusCodes.Status204NoContent)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> DeleteFileAsync(string key, [FromQuery] string bucketName)
//    {
//        var command = new DeleteFileCommand { Key = key, BucketName = bucketName };
//        var result = await _mediator.Send(command);
//        return result.ToActionResult();
//    }
//}