using Arda9FileApi.Core;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Users.GetUsersByCompany;

public class GetUsersByCompanyQuery : IRequest<Result<GetUsersByCompanyQueryResponse>>
{
    public Guid CompanyId { get; set; }
    public int Limit { get; set; } = 10;
}