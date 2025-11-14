using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Auth.GetUserInfo;

public class GetUserInfoQuery : IRequest<Result<UserInfoResponse>>
{
    public string AccessToken { get; set; } = string.Empty;
}