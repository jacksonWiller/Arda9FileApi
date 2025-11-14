using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Arda9FileApi.Domain.Entities.UserAggregate;
using Arda9FileApi.Application.DTOs;
using AutoMapper;

namespace Arda9FileApi.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDynamoDBContext context;
    private readonly ILogger<UserRepository> logger;
    private readonly IMapper mapper;

    public UserRepository(IDynamoDBContext context, ILogger<UserRepository> logger, IMapper mapper)
    {
        this.context = context;
        this.logger = logger;
        this.mapper = mapper;
    }

    public async Task<bool> CreateAsync(User user)
    {
        try
        {
            var userDto = mapper.Map<UserDto>(user);
            userDto.GSI1PK = $"EMAIL#{user.Email.Value.ToLowerInvariant()}";
            userDto.GSI2PK = $"COMPANY#{user.CompanyId}";
            
            if (!string.IsNullOrEmpty(user.CognitoSub))
            {
                userDto.GSI3PK = $"COGNITO#{user.CognitoSub}";
            }

            userDto.CreatedAt = DateTime.UtcNow;
            userDto.UpdatedAt = DateTime.UtcNow;

            await context.SaveAsync(userDto);
            logger.LogInformation("User {UserId} for company {CompanyId} is added", user.Id, user.CompanyId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist user to DynamoDB Table");
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteAsync(User user)
    {
        bool result;
        try
        {
            await context.DeleteAsync<UserDto>(user.Id);
            result = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete user from DynamoDB Table");
            result = false;
        }

        if (result) logger.LogInformation("User {UserId} is deleted", user.Id);

        return result;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        if (user == null) return false;

        try
        {
            var userDto = mapper.Map<UserDto>(user);
            userDto.GSI1PK = $"EMAIL#{user.Email.Value.ToLowerInvariant()}";
            userDto.GSI2PK = $"COMPANY#{user.CompanyId}";
            
            if (!string.IsNullOrEmpty(user.CognitoSub))
            {
                userDto.GSI3PK = $"COGNITO#{user.CognitoSub}";
            }

            userDto.UpdatedAt = DateTime.UtcNow;

            await context.SaveAsync(userDto);
            logger.LogInformation("User {UserId} for company {CompanyId} is updated", user.Id, user.CompanyId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user from DynamoDB Table");
            return false;
        }

        return true;
    }

    public async Task<User?> GetByIdAsync(Guid companyId, Guid userId)
    {
        try
        {
            var userDto = await context.LoadAsync<UserDto>(userId);

            if (userDto == null) return null;

            // Validar se o usuário pertence à company solicitada
            if (userDto.CompanyId != companyId)
            {
                logger.LogWarning("User {UserId} does not belong to company {CompanyId}", userId, companyId);
                return null;
            }

            return mapper.Map<User>(userDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get user from DynamoDB Table");
            return null;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            var queryConfig = new QueryOperationConfig
            {
                IndexName = "EmailIndex",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "GSI1PK = :email",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":email", $"EMAIL#{email.ToLowerInvariant()}" }
                    }
                },
                Limit = 1
            };

            var search = context.FromQueryAsync<UserDto>(queryConfig);
            var users = await search.GetNextSetAsync();
            var userDto = users.FirstOrDefault();

            if (userDto == null) return null;

            return mapper.Map<User>(userDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get user by email from DynamoDB Table");
            return null;
        }
    }

    public async Task<User?> GetByCognitoSubAsync(string cognitoSub)
    {
        try
        {
            var queryConfig = new QueryOperationConfig
            {
                IndexName = "CognitoIndex",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "GSI3PK = :cognitoSub",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":cognitoSub", $"COGNITO#{cognitoSub}" }
                    }
                },
                Limit = 1
            };

            var search = context.FromQueryAsync<UserDto>(queryConfig);
            var users = await search.GetNextSetAsync();
            var userDto = users.FirstOrDefault();

            if (userDto == null) return null;

            return mapper.Map<User>(userDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get user by CognitoSub from DynamoDB Table");
            return null;
        }
    }

    public async Task<IList<User>> GetUsersByCompanyAsync(Guid companyId, int limit = 10)
    {
        var result = new List<User>();

        try
        {
            if (limit <= 0) return result;

            var queryConfig = new QueryOperationConfig
            {
                IndexName = "CompanyIndex",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "GSI2PK = :companyId",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":companyId", $"COMPANY#{companyId}" }
                    }
                },
                Limit = limit
            };

            var search = context.FromQueryAsync<UserDto>(queryConfig);

            do
            {
                var dtos = await search.GetNextSetAsync();
                var users = mapper.Map<List<User>>(dtos);
                result.AddRange(users);
            }
            while (!search.IsDone && result.Count < limit);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to list users from DynamoDB Table");
            return new List<User>();
        }

        return result;
    }
}