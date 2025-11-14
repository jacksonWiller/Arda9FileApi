using Arda9UserApi.Core;
using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Users.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<Result<GetUserByEmailQueryResponse>>;