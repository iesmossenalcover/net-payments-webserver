using Application.Common.Services;
using Domain.Entities.GoogleApi;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class TasksRepository : Repository<Domain.Entities.Tasks.Task>, ITasksRepository
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TasksRepository(AppDbContext dbContext, IServiceScopeFactory serviceScopeFactory) : base(dbContext, dbContext.Tasks)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<bool> AtomiInsertTaskAsync(Domain.Entities.Tasks.Task newTask)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingTask = await dbContext.Tasks.FirstOrDefaultAsync(x =>
                        (x.Status == Domain.Entities.Tasks.TaskStatus.PENDING || x.Status == Domain.Entities.Tasks.TaskStatus.RUNNING) &&
                        x.Type == Domain.Entities.Tasks.TaskType.SYNC_USERS_TO_GOOGLE_WORKSPACE
                    , CancellationToken.None);

                    if (existingTask == null)
                    {
                        dbContext.Tasks.Add(newTask);
                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                    else
                    {
                        await transaction.CommitAsync();
                        return false;
                    }
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }
    }
}