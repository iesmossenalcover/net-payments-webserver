using Application.Common.Models;

namespace Application.Common.Services;

public interface ITransactionsService
{
    public Task InsertAndUpdateTransactionAsync(BatchUploadModel batchUploadModel, CancellationToken ct);
}