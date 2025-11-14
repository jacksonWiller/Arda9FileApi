using Arda9FileApi.Core;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Users.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<Result<GetUserByEmailQueryResponse>>;