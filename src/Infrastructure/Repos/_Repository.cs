using Application.Common.Services;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class Repository<T> : IRepository<T> where T : Entity
{
    #region constructors

    protected readonly AppDbContext _dbContext;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext dbContext, DbSet<T> dbSet)
    {
        _dbContext = dbContext;
        _dbSet = dbSet;
    }
    
    #endregion

    public async Task<T?> GetByIdAsync(long id, CancellationToken ct)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetByIdAsync(IEnumerable<long> ids, CancellationToken ct)
    {
        return await _dbSet.Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct)
    {
        return await _dbSet.ToListAsync(ct);
    }

    public async Task InsertAsync(T entity, CancellationToken ct)
    {
        _dbSet.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity, CancellationToken ct)
    {
        _dbSet.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task InsertManyAsync(IEnumerable<T> entities, CancellationToken ct)
    {
        _dbSet.AddRange(entities);
        await _dbContext.SaveChangesAsync();
    }
}