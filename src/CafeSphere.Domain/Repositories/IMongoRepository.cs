using System.Linq.Expressions;
using CafeSphere.Domain.Common;
using CafeSphere.Shared.Models;
using MongoDB.Driver;

namespace CafeSphere.Domain.Repositories;

public interface IMongoRepository<TEntity> where TEntity : BaseEntity
{
    IMongoCollection<TEntity> Collection { get; }

    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter,
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool isDescending = false,
        CancellationToken cancellationToken = default);

    Task<long> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(string id, string? deletedBy = null, CancellationToken cancellationToken = default);
}
