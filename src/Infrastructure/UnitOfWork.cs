using Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct)
    {
        // Si ja hi ha una transacció oberta, l'operació s'afegeix a la que ja hi ha.
        if (_dbContext.Database.CurrentTransaction != null)
        {
            await operation(ct);
            return;
        }

        // Sense try/catch a propòsit: si l'operació peta, el Dispose del using fa el rollback
        // i l'excepció puja tal com és. Un rollback explícit dins d'un catch pot petar ell mateix
        // (connexió caiguda) i amagar l'error original.
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        await operation(ct);
        await transaction.CommitAsync(ct);
    }
}
