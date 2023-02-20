namespace Application.Common.Services;

public interface IRepository<T> where T : Domain.Entity
{
    Task<T?> GetByIdAsync(long id, CancellationToken ct);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
    Task<IEnumerable<T>> GetByIdAsync(IEnumerable<long> ids, CancellationToken ct);
    Task InsertAsync(T entity, CancellationToken ct);
    Task InsertManyAsync(IEnumerable<T> entities, CancellationToken ct);
    Task UpdateAsync(T entity, CancellationToken ct);
    Task DeleteAsync(T entity, CancellationToken ct);
    Task DeleteManyAsync(IEnumerable<T> entities, CancellationToken ct);
}