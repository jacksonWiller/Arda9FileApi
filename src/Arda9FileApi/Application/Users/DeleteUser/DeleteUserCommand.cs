using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Users.DeleteUser;

public record DeleteUserCommand(Guid CompanyId, Guid UserId) : IRequest<Result<DeleteUserResponse>>;