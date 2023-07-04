using Application.Common.Models;

namespace Domain.Services;

public interface ITransactionsService
{
    public Task InsertAndUpdateTransactionAsync(BatchUploadModel batchUploadModel, CancellationToken ct);
}