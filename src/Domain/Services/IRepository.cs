namespace Domain.Services;

public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(long id, bool readOnly, CancellationToken ct);
    Task<IEnumerable<T>> GetByIdAsync(IEnumerable<long> ids, bool readOnly, CancellationToken ct);
    Task<IEnumerable<T>> GetAllAsync(bool readOnly, CancellationToken ct);
    Task InsertAsync(T entity, CancellationToken ct);
    Task InsertManyAsync(IEnumerable<T> entities, CancellationToken ct);
    Task UpdateAsync(T entity, CancellationToken ct);
    Task UpdateManyAsync(IEnumerable<T> entities, CancellationToken ct);
    Task DeleteAsync(T entity, CancellationToken ct);
    Task DeleteManyAsync(IEnumerable<T> entities, CancellationToken ct);
}