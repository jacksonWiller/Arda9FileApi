using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.GetUserInfo;

public class GetUserInfoQuery : IRequest<Result<UserInfoResponse>>
{
    public string AccessToken { get; set; } = string.Empty;
}