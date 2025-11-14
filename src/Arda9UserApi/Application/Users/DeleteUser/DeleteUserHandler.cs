using Arda9UserApi.Core;
using Arda9UserApi.Infrastructure.Repositories;
using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Users.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result<DeleteUserResponse>>
{
    private readonly IUserRepository repository;
    private readonly ILogger<DeleteUserHandler> logger;

    public DeleteUserHandler(IUserRepository repository, ILogger<DeleteUserHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<DeleteUserResponse>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await repository.GetByIdAsync(request.CompanyId, request.UserId);

            if (user == null)
            {
                return Result<DeleteUserResponse>.Error();
            }

            var success = await repository.DeleteAsync(user);

            if (!success)
            {
                return Result<DeleteUserResponse>.Error();
            }

            return Result<DeleteUserResponse>.Success(new DeleteUserResponse
            {
                Success = true,
                Message = "User deleted successfully"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user");
            return Result<DeleteUserResponse>.Error();
        }
    }
}