using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Users.GetUserById;

public record GetUserByIdQuery(Guid CompanyId, Guid UserId) : IRequest<Result<GetUserByIdQueryResponse>>;