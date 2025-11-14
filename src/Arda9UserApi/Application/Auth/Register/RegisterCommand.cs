using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Auth.Register;

public class RegisterCommand : IRequest<Result<RegisterResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}