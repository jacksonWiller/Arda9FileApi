using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Features.Users.GetUserByEmail;

public class GetUserByEmailQueryResponse
{
    public UserDto? User { get; set; }
}