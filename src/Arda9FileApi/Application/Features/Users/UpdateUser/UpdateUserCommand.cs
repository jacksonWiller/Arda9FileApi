using Arda9FileApi.Core;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Features.Users.UpdateUser;

public class UpdateUserCommand : IRequest<Result<UpdateUserResponse>>
{
    public Guid CompanyId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PhoneCountryCode { get; set; }
    public string? PictureUrl { get; set; }
    public string? Locale { get; set; }
    public Guid? UpdatedBy { get; set; }
}