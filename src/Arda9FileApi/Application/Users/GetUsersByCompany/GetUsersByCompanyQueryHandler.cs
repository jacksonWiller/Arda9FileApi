using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Core;
using Arda9FileApi.Infrastructure.Repositories;
using Ardalis.Result;
using AutoMapper;
using MediatR;

namespace Arda9FileApi.Application.Users.GetUsersByCompany;

public class GetUsersByCompanyQueryHandler : IRequestHandler<GetUsersByCompanyQuery, Result<GetUsersByCompanyQueryResponse>>
{
    private readonly IUserRepository repository;
    private readonly IMapper mapper;    
    private readonly ILogger<GetUsersByCompanyQueryHandler> logger;

    public GetUsersByCompanyQueryHandler(IUserRepository repository, IMapper mapper, ILogger<GetUsersByCompanyQueryHandler> logger)
    {
        this.repository = repository;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<Result<GetUsersByCompanyQueryResponse>> Handle(GetUsersByCompanyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await repository.GetUsersByCompanyAsync(request.CompanyId, request.Limit);
            var userDtos = mapper.Map<List<UserDto>>(users);

            return Result<GetUsersByCompanyQueryResponse>.Success(new GetUsersByCompanyQueryResponse
            {
                Users = userDtos,
                Count = userDtos.Count
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting users by company");
            return Result<GetUsersByCompanyQueryResponse>.Error();
        }
    }
}