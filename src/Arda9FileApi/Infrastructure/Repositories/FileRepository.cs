using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<FileRepository> _logger;

    public FileRepository(IDynamoDBContext context, ILogger<FileRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FileMetadataDto?> GetByIdAsync(Guid fileId)
    {
        try
        {
            return await _context.LoadAsync<FileMetadataDto>($"FILE#{fileId}", "METADATA");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivo por ID: {FileId}", fileId);
            throw;
        }
    }

    public async Task<FileMetadataDto?> GetByS3KeyAsync(string s3Key)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("S3Key", ScanOperator.Equal, s3Key),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false)
            };

            var search = _context.ScanAsync<FileMetadataDto>(conditions);
            var results = await search.GetNextSetAsync();
            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivo por S3Key: {S3Key}", s3Key);
            throw;
        }
    }

    public async Task<List<FileMetadataDto>> GetByCompanyIdAsync(Guid companyId)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("CompanyId", ScanOperator.Equal, companyId),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false)
            };

            var search = _context.ScanAsync<FileMetadataDto>(conditions);
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivos por CompanyId: {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<List<FileMetadataDto>> GetByBucketNameAsync(string bucketName)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("BucketName", ScanOperator.Equal, bucketName),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false)
            };

            var search = _context.ScanAsync<FileMetadataDto>(conditions);
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivos por BucketName: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<List<FileMetadataDto>> GetAllAsync()
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("IsDeleted", ScanOperator.Equal, false)
            };

            var search = _context.ScanAsync<FileMetadataDto>(conditions);
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os arquivos");
            throw;
        }
    }

    public async Task CreateAsync(FileMetadataDto fileMetadata)
    {
        try
        {
            await _context.SaveAsync(fileMetadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar arquivo: {FileName}", fileMetadata.FileName);
            throw;
        }
    }

    public async Task UpdateAsync(FileMetadataDto fileMetadata)
    {
        try
        {
            fileMetadata.UpdatedAt = DateTime.UtcNow;
            await _context.SaveAsync(fileMetadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar arquivo: {FileId}", fileMetadata.FileId);
            throw;
        }
    }

    public async Task DeleteAsync(Guid fileId)
    {
        try
        {
            var file = await GetByIdAsync(fileId);
            if (file != null)
            {
                file.IsDeleted = true;
                file.UpdatedAt = DateTime.UtcNow;
                await _context.SaveAsync(file);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar arquivo: {FileId}", fileId);
            throw;
        }
    }
}