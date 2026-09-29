namespace Domain.Services;

/// <summary>
/// Permet agrupar diverses operacions de repositoris en una sola transacció, ja que cada
/// repositori fa el seu propi SaveChanges i sense això una operació a mitges deixa dades incoherents.
/// </summary>
public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct);
}
