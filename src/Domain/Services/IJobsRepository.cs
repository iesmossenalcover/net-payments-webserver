using Domain.Entities.Jobs;

namespace Domain.Services;

public interface IJobsRepository : IRepository<Job>
{
    // Inserts the job if there is not taks of same type as pending or runing.
    // Returns true if inserted

    /// <summary>
    /// Inserts the job if there is no taks with same type with status pending or running.
    /// </summary>
    /// <param name="newJob">New job to be inserted</param>
    /// <returns><c>true</c> if new job is inserted, otherwise <c>false</c>.</returns>
    public Task<bool> AtomicInsertJobAsync(Job newJob);

    public Task<IEnumerable<Job>> GetLastOfEachType(CancellationToken ct);
}