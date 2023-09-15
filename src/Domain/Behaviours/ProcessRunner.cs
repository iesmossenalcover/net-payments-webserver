using Domain.Entities.Jobs;
using Domain.Entities.Logs;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Behaviours;

public interface IProcess
{
    Task Run(IServiceProvider serviceProvider, Log log, CancellationToken ct);
}

public class ProcessRunner
{
    private readonly IServiceProvider _serviceProvider;

    public ProcessRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Start(IProcess process, long jobId)
    {
        _ = Task.Run(async () =>
        {
            CancellationToken ct = CancellationToken.None;

            // IOC
            using var scope = _serviceProvider.CreateAsyncScope();
            ILogStore logStore = scope.ServiceProvider.GetRequiredService<ILogStore>();
            IJobsRepository jobsRepository = scope.ServiceProvider.GetRequiredService<IJobsRepository>();

            Job? job = await jobsRepository.GetByIdAsync(jobId, ct);
            if (job == null) return;

            job.Status = JobStatus.RUNNING;
            await jobsRepository.UpdateAsync(job, ct);

            Log log = new();
            await process.Run(_serviceProvider, log, ct);
            
            LogStoreInfo logStoreInfo = await logStore.Save(log);

            job.Log = logStoreInfo;
            job.End = DateTimeOffset.UtcNow;
            job.Status = JobStatus.FINISHED;

            await jobsRepository.UpdateAsync(job, ct);
        });
    }
}