using Ardalis.Result;
using MediatR;
using System.Text.Json.Serialization;

namespace Arda9File.Application.Application.Folders.Queries.GetFolders;

public class GetFoldersQuery : IRequest<Result<GetFoldersResponse>>
{
    [JsonIgnore]
    public Guid? ParentId { get; set; }
    public bool IncludeEmpty { get; set; } = true;
    public int Depth { get; set; } = 1;
}
