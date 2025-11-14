using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Application.Users.GetUsersByCompany;

public class GetUsersByCompanyQueryResponse
{
    public List<UserDto> Users { get; set; } = [];
    public int Count { get; set; }
}