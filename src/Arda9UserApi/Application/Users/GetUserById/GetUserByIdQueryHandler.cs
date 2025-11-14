using Arda9UserApi.Application.DTOs;
using Arda9UserApi.Core;
using Arda9UserApi.Infrastructure.Repositories;
using Ardalis.Result;
using AutoMapper;
using MediatR;

namespace Arda9UserApi.Application.Users.GetUserById;

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