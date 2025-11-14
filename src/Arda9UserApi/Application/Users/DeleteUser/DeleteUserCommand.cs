using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Users.DeleteUser;

public record DeleteUserCommand(Guid CompanyId, Guid UserId) : IRequest<Result<DeleteUserResponse>>;