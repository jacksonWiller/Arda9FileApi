using Arda9UserApi.Domain.Entities.UserAggregate;

namespace Arda9UserApi.Infrastructure.Repositories;

/// <summary>
/// Repositório de User para operações CRUD no DynamoDB
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Adiciona um novo user ao DynamoDB
    /// </summary>
    /// <param name="user">Entidade de domínio User</param>
    /// <returns>success/failure</returns>
    Task<bool> CreateAsync(User user);

    /// <summary>
    /// Remove um user existente do DynamoDB
    /// </summary>
    /// <param name="user">Entidade de domínio User</param>
    /// <returns>success/failure</returns>
    Task<bool> DeleteAsync(User user);

    /// <summary>
    /// Lista users de uma company do DynamoDB com limite de items (padrão=10)
    /// </summary>
    /// <param name="companyId">ID da company</param>
    /// <param name="limit">limite (padrão=10)</param>
    /// <returns>Coleção de users do domínio</returns>
    Task<IList<User>> GetUsersByCompanyAsync(Guid companyId, int limit = 10);

    /// <summary>
    /// Obtém um user pela chave composta
    /// </summary>
    /// <param name="companyId">ID da company</param>
    /// <param name="userId">ID do user</param>
    /// <returns>Entidade de domínio User</returns>
    Task<User?> GetByIdAsync(Guid companyId, Guid userId);

    /// <summary>
    /// Obtém um user pelo email
    /// </summary>
    /// <param name="email">Email do user</param>
    /// <returns>Entidade de domínio User</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Obtém um user pelo CognitoSub
    /// </summary>
    /// <param name="cognitoSub">CognitoSub do user</param>
    /// <returns>Entidade de domínio User</returns>
    Task<User?> GetByCognitoSubAsync(string cognitoSub);

    /// <summary>
    /// Atualiza o conteúdo de um user
    /// </summary>
    /// <param name="user">Entidade de domínio User</param>
    /// <returns>success/failure</returns>
    Task<bool> UpdateAsync(User user);
}