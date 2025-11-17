using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Infrastructure.Repositories;
using MediatR;

namespace Arda9FileApi.Application.Files.Queries.GetFileById;

public class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQuery, Result<FileMetadataDto>>
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

    public async Task<Result<FileMetadataDto>> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _repository.GetByIdAsync(request.FileId);
            
            if (file == null || file.IsDeleted)
            {
                _logger.LogWarning("File {FileId} not found", request.FileId);
                return Result<FileMetadataDto>.NotFound();
            }

            if (file.CompanyId != request.TenantId)
            {
                _logger.LogWarning("File {FileId} does not belong to tenant {TenantId}", 
                    request.FileId, request.TenantId);
                return Result<FileMetadataDto>.Forbidden();
            }

            return Result<FileMetadataDto>.Success(file);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file {FileId}", request.FileId);
            return Result<FileMetadataDto>.Error();
        }
    }
}