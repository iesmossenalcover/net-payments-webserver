using Domain.Entities.Jobs;
using Domain.Entities.Logs;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Services;

public interface IProcess
{
    Task Run(IServiceScopeFactory serviceProvider, Log log, CancellationToken ct);
}

public class ProcessRunner
{
    private readonly IServiceScopeFactory _serviceProvider;

    public ProcessRunner(IServiceScopeFactory serviceProvider)
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
            log.Add("Starting process...");
            try
            {
                await process.Run(_serviceProvider, log, ct);
            }
            catch (Exception e)
            {
                log.Add(e.Message);
            }

            log.Add("Process finished");

            LogStoreInfo logStoreInfo = await logStore.Save(log);

            job.Log = logStoreInfo;
            job.End = DateTimeOffset.UtcNow;
            job.Status = JobStatus.FINISHED;

            await jobsRepository.UpdateAsync(job, ct);
        });
    }
}