using System.Linq.Expressions;
using CafeSphere.Domain.Common;
using CafeSphere.Domain.Repositories;
using CafeSphere.Persistence.Context;
using CafeSphere.Shared.Models;
using MongoDB.Driver;

namespace CafeSphere.Persistence.Repositories;

public class MongoRepository<TEntity> : IMongoRepository<TEntity> where TEntity : BaseEntity
{
    private readonly IMongoDbContext _context;

    public IMongoCollection<TEntity> Collection { get; }

    public MongoRepository(IMongoDbContext context)
    {
        _context = context;
        Collection = _context.GetCollection<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.And(
            Builders<TEntity>.Filter.Eq(x => x.Id, id),
            Builders<TEntity>.Filter.Eq(x => x.IsDeleted, false)
        );

        return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        var combinedFilter = Builders<TEntity>.Filter.And(
            Builders<TEntity>.Filter.Where(filter),
            Builders<TEntity>.Filter.Eq(x => x.IsDeleted, false)
        );

        return await Collection.Find(combinedFilter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(x => x.IsDeleted, false);
        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        var combinedFilter = Builders<TEntity>.Filter.And(
            Builders<TEntity>.Filter.Where(filter),
            Builders<TEntity>.Filter.Eq(x => x.IsDeleted, false)
        );

        return await Collection.Find(combinedFilter).ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter,
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool isDescending = false,
        CancellationToken cancellationToken = default)
    {
        var combinedFilter = Builders<TEntity>.Filter.And(
            Builders<TEntity>.Filter.Where(filter),
            Builders<TEntity>.Filter.Eq(x => x.IsDeleted, false)
        );

        var totalCount = await Collection.CountDocumentsAsync(combinedFilter, cancellationToken: cancellationToken);

        var fluentFind = Collection.Find(combinedFilter);

        if (orderBy != null)
        {
            fluentFind = isDescending ? fluentFind.SortByDescending(orderBy) : fluentFind.SortBy(orderBy);
        }

        var skip = (pageNumber - 1) * pageSize;
        var items = await fluentFind.Skip(skip).Limit(pageSize).ToListAsync(cancellationToken);

        return PagedResult<TEntity>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<long> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        var softDeleteFilter = Builders<TEntity>.Filter.Eq(x => x.IsDeleted, false);

        var finalFilter = filter == null
            ? softDeleteFilter
            : Builders<TEntity>.Filter.And(Builders<TEntity>.Filter.Where(filter), softDeleteFilter);

        return await Collection.CountDocumentsAsync(finalFilter, cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        var count = await CountAsync(filter, cancellationToken);
        return count > 0;
    }

    public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public async Task InsertManyAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        var now = DateTime.UtcNow;
        foreach (var entity in entityList)
        {
            entity.CreatedAt = now;
        }

        await Collection.InsertManyAsync(entityList, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, entity.Id);
        await Collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
        await Collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task SoftDeleteAsync(string id, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
        var update = Builders<TEntity>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Set(x => x.UpdatedBy, deletedBy);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}
