using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Microsoft.Extensions.Logging;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Files.Queries.GetFileById;

public class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQuery, Result<FileMetadataModel>>
{
    private readonly IFileRepository _repository;
    private readonly ILogger<GetFileByIdQueryHandler> _logger;

    public GetFileByIdQueryHandler(
        IFileRepository repository,
        ILogger<GetFileByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<FileMetadataModel>> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _repository.GetByIdAsync(request.FileId);
            
            if (file == null || file.IsDeleted)
            {
                _logger.LogWarning("File {FileId} not found", request.FileId);
                return Result<FileMetadataModel>.NotFound();
            }

            if (file.TenantId != request.TenantId)
            {
                _logger.LogWarning("File {FileId} does not belong to tenant {TenantId}", 
                    request.FileId, request.TenantId);
                return Result<FileMetadataModel>.Forbidden();
            }

            return Result<FileMetadataModel>.Success(file);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file {FileId}", request.FileId);
            return Result<FileMetadataModel>.Error();
        }
    }
}