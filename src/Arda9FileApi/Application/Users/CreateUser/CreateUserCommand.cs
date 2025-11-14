using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Users.CreateUser;

public class CreateUserCommand : IRequest<Result<CreateUserResponse>>
{
    public Guid CompanyId { get; set; }
    public Guid? SubCompanyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PhoneCountryCode { get; set; }
    public List<Guid>? Roles { get; set; }
    public string? PictureUrl { get; set; }
    public string? Locale { get; set; }         
    public string? CognitoSub { get; set; }
    public string? Username { get; set; }
    public Guid? CreatedBy { get; set; }
}