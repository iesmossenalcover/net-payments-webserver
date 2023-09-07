using Application.Common.Models;

namespace Domain.Services;

public interface ITransactionsService
{
    public Task<TransactionResult<string>> InsertAndUpdateTransactionAsync(BatchUploadModel batchUploadModel);
}

public record TransactionResult<T>(bool Ok, string? Error, T? Data);