using Arda9UserApi.Core;
using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Users.GetUsersByCompany;

public class GetUsersByCompanyQuery : IRequest<Result<GetUsersByCompanyQueryResponse>>
{
    public Guid CompanyId { get; set; }
    public int Limit { get; set; } = 10;
}