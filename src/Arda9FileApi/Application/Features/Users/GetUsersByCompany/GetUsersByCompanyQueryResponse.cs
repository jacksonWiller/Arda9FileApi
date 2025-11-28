using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Features.Users.GetUsersByCompany;

public class GetUsersByCompanyQueryResponse
{
    public List<UserDto> Users { get; set; } = [];
    public int Count { get; set; }
}