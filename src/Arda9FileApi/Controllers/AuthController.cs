using Arda9FileApi.Application.Auth.ChangePassword;
using Arda9FileApi.Application.Auth.ConfirmEmail;
using Arda9FileApi.Application.Auth.ForgotPassword;
using Arda9FileApi.Application.Auth.GetUserInfo;
using Arda9FileApi.Application.Auth.Login;
using Arda9FileApi.Application.Auth.Logout;
using Arda9FileApi.Application.Auth.RefreshToken;
using Arda9FileApi.Application.Auth.Register;
using Arda9FileApi.Application.Auth.ResendCode;
using Arda9FileApi.Application.Auth.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arda9FileApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator mediator;

    public AuthController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Registra um novo usuário no AWS Cognito
    /// </summary>
    /// <param name="command">Dados do novo usuário</param>
    /// <returns>Informações do usuário criado</returns>
    /// <response code="200">Usuário registrado com sucesso</response>
    /// <response code="400">Parâmetros inválidos</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { Success = false, Message = result.Errors.FirstOrDefault() });
    }

    /// <summary>
    /// Confirma o registro do usuário com o código enviado por email
    /// </summary>
    /// <param name="command">Email e código de confirmação</param>
    /// <returns>Status da confirmação</returns>
    /// <response code="200">Email confirmado com sucesso</response>
    /// <response code="400">Código inválido ou expirado</response>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ConfirmEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { Success = false, Message = result.Errors.FirstOrDefault() });
    }

    /// <summary>
    /// Reenvia o código de confirmação para o email do usuário
    /// </summary>
    /// <param name="command">Email do usuário</param>
    /// <returns>Status do reenvio</returns>
    /// <response code="200">Código reenviado com sucesso</response>
    /// <response code="400">Erro ao reenviar código</response>
    [HttpPost("resend-code")]
    [ProducesResponseType(typeof(ResendCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmationCodeAsync([FromBody] ResendCodeCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { Success = false, Message = result.Errors.FirstOrDefault() });
    }

    /// <summary>
    /// Realiza login do usuário no AWS Cognito
    /// </summary>
    /// <param name="command">Credenciais do usuário</param>
    /// <returns>Token de autenticação</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="400">Parâmetros inválidos</response>
    /// <response code="401">Credenciais inválidas</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginCommand command)
    {
        var result = await mediator.Send(command);
        
        if (result.IsSuccess)
            return Ok(result.Value);
        
        var errorMessage = result.Errors.FirstOrDefault() ?? string.Empty;
        return errorMessage.Contains("incorretos") ? Unauthorized(new { Success = false, Message = errorMessage }) 
            : BadRequest(new { Success = false, Message = errorMessage });
    }

    /// <summary>
    /// Renova o token de acesso usando o refresh token
    /// </summary>
    /// <param name="command">Refresh token</param>
    /// <returns>Novos tokens de acesso</returns>
    /// <response code="200">Token renovado com sucesso</response>
    /// <response code="401">Refresh token inválido</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { Success = false, Message = result.Errors.FirstOrDefault() });
    }

    /// <summary>
    /// Inicia o processo de recuperação de senha
    /// </summary>
    /// <param name="command">Email do usuário</param>
    /// <returns>Status do envio do código</returns>
    /// <response code="200">Código enviado com sucesso</response>
    /// <response code="400">Erro ao enviar código</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result.Value); // Sempre retorna sucesso por segurança
    }

    /// <summary>
    /// Confirma a redefinição de senha com o código enviado
    /// </summary>
    /// <param name="command">Email, código e nova senha</param>
    /// <returns>Status da redefinição</returns>
    /// <response code="200">Senha redefinida com sucesso</response>
    /// <response code="400">Código inválido ou senha inválida</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { Success = false, Message = result.Errors.FirstOrDefault() });
    }

    /// <summary>
    /// Altera a senha do usuário autenticado
    /// </summary>
    /// <param name="request">Senha atual e nova senha</param>
    /// <returns>Status da alteração</returns>
    /// <response code="200">Senha alterada com sucesso</response>
    /// <response code="400">Senha atual incorreta</response>
    /// <response code="401">Não autenticado</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordCommand command)
    {
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { Success = false, Message = result.Errors.FirstOrDefault() });
    }

    /// <summary>
    /// Realiza logout do usuário (revoga tokens)
    /// </summary>
    /// <returns>Status do logout</returns>
    /// <response code="200">Logout realizado com sucesso</response>
    /// <response code="401">Não autenticado</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAsync()
    {
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        
        var command = new LogoutCommand { AccessToken = accessToken };
        var result = await mediator.Send(command);
        
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, new { Success = false, Message = result.Errors.FirstOrDefault() });
    }

    /// <summary>
    /// Obtém informações do usuário autenticado
    /// </summary>
    /// <returns>Dados do usuário</returns>
    /// <response code="200">Informações obtidas com sucesso</response>
    /// <response code="401">Não autenticado</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserInfoAsync()
    {
        var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        
        var query = new GetUserInfoQuery { AccessToken = accessToken };
        var result = await mediator.Send(query);
        
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, new { Success = false, Message = result.Errors.FirstOrDefault() });
    }
}