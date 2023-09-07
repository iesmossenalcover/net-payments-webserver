using Application.Common.Models;
using Domain.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure;

public class TransactionsService : Domain.Services.ITransactionsService
{
    private readonly AppDbContext _dbContext;

    public TransactionsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransactionResult<string>> InsertAndUpdateTransactionAsync(BatchUploadModel batchUploadModel)
    {
        try
        {
            _dbContext.Groups.AddRange(batchUploadModel.NewGroups);
            _dbContext.Groups.UpdateRange(batchUploadModel.ExistingGroups);
            _dbContext.People.AddRange(batchUploadModel.NewPeople);
            _dbContext.People.UpdateRange(batchUploadModel.ExistingPeople);
            _dbContext.PersonGroupCourses.RemoveRange(batchUploadModel.PersonGroupCoursesToDelete);
            _dbContext.PersonGroupCourses.AddRange(batchUploadModel.NewPersonGroupCourses);
            _dbContext.PersonGroupCourses.UpdateRange(batchUploadModel.ExistingPersonGroupCourses);
            await _dbContext.SaveChangesAsync(CancellationToken.None);
            return new TransactionResult<string>(true, null, null);
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException is PostgresException sqlException)
            {
                return new TransactionResult<string>(false, sqlException.Detail, null);
            }

            throw;
        }
    }
}