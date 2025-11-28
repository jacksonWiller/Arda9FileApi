using Arda9FileApi.Core;
using Arda9FileApi.Models;
using Arda9FileApi.Repositories;
using Ardalis.Result;
using AutoMapper;
using MediatR;

namespace Arda9FileApi.Application.Features.Users.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<GetUserByIdQueryResponse>>
{
    private readonly IUserRepository repository;
    private readonly IMapper mapper;
    private readonly ILogger<GetUserByIdQueryHandler> logger;

    public GetUserByIdQueryHandler(IUserRepository repository, IMapper mapper, ILogger<GetUserByIdQueryHandler> logger)
    {
        this.repository = repository;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<Result<GetUserByIdQueryResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await repository.GetByIdAsync(request.CompanyId, request.UserId);

            if (user == null)
            {
                return Result<GetUserByIdQueryResponse>.Error();
            }

            var userDto = mapper.Map<UserDto>(user);

            return Result<GetUserByIdQueryResponse>.Success(new GetUserByIdQueryResponse { User = userDto });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user by id");
            return Result<GetUserByIdQueryResponse>.Error();
        }
    }
}