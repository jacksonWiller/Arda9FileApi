using Arda9UserApi.Application.DTOs;
using Arda9UserApi.Core;
using Arda9UserApi.Infrastructure.Repositories;
using Ardalis.Result;
using AutoMapper;
using MediatR;

namespace Arda9UserApi.Application.Users.GetUserByEmail;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<GetUserByEmailQueryResponse>>
{
    private readonly IUserRepository repository;
    private readonly IMapper mapper;
    private readonly ILogger<GetUserByEmailQueryHandler> logger;

    public GetUserByEmailQueryHandler(IUserRepository repository, IMapper mapper, ILogger<GetUserByEmailQueryHandler> logger)
    {
        this.repository = repository;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<Result<GetUserByEmailQueryResponse>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await repository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                return Result<GetUserByEmailQueryResponse>.Error();
            }

            var userDto = mapper.Map<UserDto>(user);

            return Result<GetUserByEmailQueryResponse>.Success(new GetUserByEmailQueryResponse { User = userDto });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user by email");
            return Result<GetUserByEmailQueryResponse>.Error();
        }
    }
}