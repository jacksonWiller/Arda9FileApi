using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.ChangePassword;

public class ChangePasswordCommand : IRequest<Result<ChangePasswordResponse>>
{

    private string AccessToken = string.Empty;
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;

    public string GetAcessToken()
    {
        return AccessToken;
    }

    public void SetAccessToken(string token)
    {
        AccessToken = token;
    }
}