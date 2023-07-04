using Application.Common.Models;

namespace Infrastructure;

public class TransactionsService : Domain.Services.ITransactionsService
{
    private readonly AppDbContext _dbContext;

    public TransactionsService(AppDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task InsertAndUpdateTransactionAsync(BatchUploadModel batchUploadModel, CancellationToken ct)
    {
        _dbContext.Groups.AddRange(batchUploadModel.NewGroups);
        _dbContext.Groups.UpdateRange(batchUploadModel.ExistingGroups);
        _dbContext.People.AddRange(batchUploadModel.NewPeople);
        _dbContext.People.UpdateRange(batchUploadModel.ExistingPeople);
        _dbContext.PersonGroupCourses.AddRange(batchUploadModel.NewPersonGroupCourses);
        _dbContext.PersonGroupCourses.UpdateRange(batchUploadModel.ExistingPersonGroupCourses);
        await _dbContext.SaveChangesAsync();
    }
}