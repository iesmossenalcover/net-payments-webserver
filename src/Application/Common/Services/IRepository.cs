namespace Application.Common.Services;

public interface IRepository<T> where T : Domain.Entity
{
    Task<T?> GetByIdAsync(long id, CancellationToken ct);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
    IQueryable<T> GetByIdAsync(IEnumerable<long> ids, CancellationToken ct);
    Task InsertAsync(T entity, CancellationToken ct);
    Task UpdateAsync(T entity, CancellationToken ct);
    Task DeleteAsync(T entity, CancellationToken ct);
}