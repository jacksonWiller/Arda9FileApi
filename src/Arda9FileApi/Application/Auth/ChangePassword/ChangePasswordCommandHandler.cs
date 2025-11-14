using Amazon.CognitoIdentityProvider.Model;
using Arda9FileApi.Application.Extensions;
using Arda9FileApi.Application.Services;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<ChangePasswordResponse>>
{
    private readonly IAuthService authService;
    private readonly ILogger<ChangePasswordCommandHandler> logger;

    public ChangePasswordCommandHandler(IAuthService authService, ILogger<ChangePasswordCommandHandler> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    public async Task<Result<ChangePasswordResponse>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await authService.ChangePasswordAsync(request.GetAcessToken(), request.OldPassword, request.NewPassword);

            return Result.Success(new ChangePasswordResponse
            {
                Success = true,
                Message = "Senha alterada com sucesso"
            });
        }
        catch (NotAuthorizedException)
        {
            return ResultError.Error("Senha atual incorreta");
        }
        catch (InvalidPasswordException ex)
        {
            return ResultError.Error($"Nova senha inválida: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing password");
            return ResultError.Error("Erro ao alterar senha");
        }
    }
}