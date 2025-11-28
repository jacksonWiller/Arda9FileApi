using Arda9FileApi.Domain.ValueObjects;
using Arda9FileApi.Core;
using Ardalis.Result;
using AutoMapper;
using MediatR;
using Arda9FileApi.Repositories;
using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Features.Users.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UpdateUserResponse>>
{
    private readonly IUserRepository repository;
    private readonly IMapper mapper;
    private readonly ILogger<UpdateUserHandler> logger;

    public UpdateUserHandler(IUserRepository repository, IMapper mapper, ILogger<UpdateUserHandler> logger)
    {
        this.repository = repository;
        this.mapper = mapper;
        this.logger = logger;
    }

    public async Task<Result<UpdateUserResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await repository.GetByIdAsync(request.CompanyId, request.Id);

            if (user == null)
            {
                return Result<UpdateUserResponse>.Error();
            }

            var name = new PersonName(request.Name);

            Phone? phone = null;
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                phone = new Phone(request.PhoneNumber, request.PhoneCountryCode ?? "+55");
            }

            Url? pictureUrl = null;
            if (!string.IsNullOrWhiteSpace(request.PictureUrl))
            {
                pictureUrl = new Url(request.PictureUrl);
            }

            user.Update(name, phone, pictureUrl, request.Locale, request.UpdatedBy);

            var success = await repository.UpdateAsync(user);

            if (!success)
            {
                return Result<UpdateUserResponse>.Error();
            }

            var userDto = mapper.Map<UserDto>(user);

            return Result<UpdateUserResponse>.Success(new UpdateUserResponse { User = userDto });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user");
            return Result<UpdateUserResponse>.Error();
        }
    }
}