using Arda9UserApi.Api.Extensions;
using Arda9UserApi.Application.Users.CreateUser;
using Arda9UserApi.Application.Users.DeleteUser;
using Arda9UserApi.Application.Users.GetUserByEmail;
using Arda9UserApi.Application.Users.GetUserById;
using Arda9UserApi.Application.Users.GetUsersByCompany;
using Arda9UserApi.Application.Users.UpdateUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arda9UserApi.Controllers;

[Route("api/companies/{companyId}/[controller]")]
[ApiController]
//[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtém uma lista de usuários de uma empresa com limite especificado
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetUsersByCompanyQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(Guid companyId, [FromQuery] int limit = 10)
    {
        var query = new GetUsersByCompanyQuery { CompanyId = companyId, Limit = limit };
        var result = await _mediator.Send(query);
        return result.ToActionResult();
    }

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(Guid companyId, [FromBody] CreateUserCommand command)
    {
        command.CompanyId = companyId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém um usuário específico pelo ID
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(GetUserByIdQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid companyId, Guid userId)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(companyId, userId));
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém um usuário específico pelo email
    /// </summary>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(GetUserByEmailQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var result = await _mediator.Send(new GetUserByEmailQuery(email));
        return result.ToActionResult();
    }

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(UpdateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid companyId, Guid userId, [FromBody] UpdateUserCommand command)
    {
        command.CompanyId = companyId;
        command.Id = userId;
        var result = await _mediator.Send(command);
        return result.ToActionResult();
    }

    /// <summary>
    /// Remove um usuário
    /// </summary>
    [HttpDelete("{userId}")]
    [ProducesResponseType(typeof(DeleteUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid companyId, Guid userId)
    {
        var result = await _mediator.Send(new DeleteUserCommand(companyId, userId));
        return result.ToActionResult();
    }
}