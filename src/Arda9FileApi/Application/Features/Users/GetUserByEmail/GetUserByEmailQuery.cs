using Arda9FileApi.Core;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Features.Users.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<Result<GetUserByEmailQueryResponse>>;