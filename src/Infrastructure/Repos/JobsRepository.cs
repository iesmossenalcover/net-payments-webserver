
using Domain.Entities.Jobs;
using Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repos;

public class JobsRepository : Repository<Job>, IJobsRepository
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public JobsRepository(AppDbContext dbContext, IServiceScopeFactory serviceScopeFactory) : base(dbContext, dbContext.Jobs)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<bool> AtomicInsertJobAsync(Job newTask)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var existingTask = await dbContext.Jobs.FirstOrDefaultAsync(x =>
                    x.Type == newTask.Type // Same type task
                    &&
                    (x.Status == JobStatus.PENDING || x.Status == JobStatus.RUNNING) // Check for Pending or runing status
            , CancellationToken.None);

            if (existingTask == null)
            {
                dbContext.Jobs.Add(newTask);
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

    public async Task<IEnumerable<Job>> GetLastOfEachType(CancellationToken ct)
    {
        return await _dbSet
            .GroupBy(x => x.Type)
            .Select(x => x.OrderByDescending(y => y.Start).First())
            .ToListAsync();
    }
}