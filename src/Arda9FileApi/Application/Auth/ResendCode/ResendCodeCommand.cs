using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.ResendCode;

public class ResendCodeCommand : IRequest<Result<ResendCodeResponse>>
{
    public string Email { get; set; } = string.Empty;
}