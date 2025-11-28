using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Features.Users.GetUserById;

public record GetUserByIdQuery(Guid CompanyId, Guid UserId) : IRequest<Result<GetUserByIdQueryResponse>>;