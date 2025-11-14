using Arda9UserApi.Application.DTOs;

namespace Arda9UserApi.Application.Users.GetUsersByCompany;

public class GetUsersByCompanyQueryResponse
{
    public List<UserDto> Users { get; set; } = [];
    public int Count { get; set; }
}