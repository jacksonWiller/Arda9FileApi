using Arda9UserApi.Application.DTOs;

namespace Arda9UserApi.Application.Users.GetUserByEmail;

public class GetUserByEmailQueryResponse
{
    public UserDto? User { get; set; }
}