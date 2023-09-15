using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using MediatR;
using Domain.Entities.Jobs;
using Domain.ValueObjects;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record SuspendGoogleWorkspaceCommand() : IRequest<Response<SuspendGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record SuspendGoogleWorkspaceCommandVm(bool ok);

// Handler
public class SuspendGoogleWorkspaceCommandHandler : IRequestHandler<SuspendGoogleWorkspaceCommand, Response<SuspendGoogleWorkspaceCommandVm>>
{
    #region props
    private readonly IJobsRepository _jobsRepository;
    private readonly IServiceProvider _serviceProvider;

    public SuspendGoogleWorkspaceCommandHandler(IJobsRepository jobsRepository, IServiceProvider serviceProvider)
    {
        _jobsRepository = jobsRepository;
        _serviceProvider = serviceProvider;
    }

    #endregion

    public async Task<Response<SuspendGoogleWorkspaceCommandVm>> Handle(SuspendGoogleWorkspaceCommand request, CancellationToken ct)
    {
        // Start Task and try to save task
        var task = new Job()
        {
            Status = JobStatus.RUNNING,
            Start = DateTimeOffset.UtcNow,
            Type = JobType.SUSPEND_GOOGLE_WORKSPACE,
        };

        var queuedTask = await _jobsRepository.AtomicInsertJobAsync(task);
        if (!queuedTask)
        {
            return Response<SuspendGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "Ja hi ha una tasca del mateix tipus iniciada.");
        }

        long taskId = task.Id;

        _ = Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            IGoogleAdminApi googleAdminApi = scope.ServiceProvider.GetRequiredService<IGoogleAdminApi>();
            IOUGroupRelationsRepository oUGroupRelationsRepository = scope.ServiceProvider.GetRequiredService<IOUGroupRelationsRepository>();
            IJobsRepository jobsRepository = scope.ServiceProvider.GetRequiredService<IJobsRepository>();
            ILogStore logStore = scope.ServiceProvider.GetRequiredService<ILogStore>();

            ct = CancellationToken.None;
            var log = new Log();
            log.Add("Inici tasca");

            IEnumerable<UoGroupRelation> ouRelations = await oUGroupRelationsRepository.GetAllAsync(ct);
            IEnumerable<string> pendings = ouRelations.Select(x => x.OldOU).Distinct();


            foreach (var ou in pendings)
            {
                GoogleApiResult<bool> result = await googleAdminApi.SetSuspendByOU(ou, true, false);
                if (!result.Success)
                {
                    log.Add($"OU {ou} - [Error] {result.ErrorMessage ?? "No s'ha pogut processar"}");
                }
                else
                {
                    log.Add($"OU: {ou} - [OK]");
                }
            }

            log.Add("Fi tasca");
            var logStoreInfo = await logStore.Save(log);
            task = await jobsRepository.GetByIdAsync(taskId, ct);
            if (task == null) return;
            task.Log = logStoreInfo;
            task.End = DateTimeOffset.UtcNow;
            task.Status = JobStatus.FINISHED;
            await jobsRepository.UpdateAsync(task, ct);
        });

        return Response<SuspendGoogleWorkspaceCommandVm>.Ok(new SuspendGoogleWorkspaceCommandVm(true));
    }
}