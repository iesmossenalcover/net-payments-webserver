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
    private readonly ProcessRunner _processRunner;

    public SuspendGoogleWorkspaceCommandHandler(IJobsRepository jobsRepository, ProcessRunner processRunner)
    {
        _jobsRepository = jobsRepository;
        _processRunner = processRunner;
    }

    #endregion

    public async Task<Response<SuspendGoogleWorkspaceCommandVm>> Handle(SuspendGoogleWorkspaceCommand request, CancellationToken ct)
    {
        // Queue the job if it is possible.
        var job = new Job()
        {
            Status = JobStatus.PENDING,
            Start = DateTimeOffset.UtcNow,
            Type = JobType.SUSPEND_GOOGLE_WORKSPACE,
        };
        var queuedTask = await _jobsRepository.AtomicInsertJobAsync(job);
        if (!queuedTask)
        {
            return Response<SuspendGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "Ja hi ha una tasca del mateix tipus iniciada.");
        }

        var process = new SuspendGoogleWorkspaceProcess();
        _processRunner.Start(process, job.Id);

        return Response<SuspendGoogleWorkspaceCommandVm>.Ok(new SuspendGoogleWorkspaceCommandVm(true));
    }
}

public class SuspendGoogleWorkspaceProcess : IProcess
{
    public async Task Run(IServiceScopeFactory serviceProvider, Log log, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        IGoogleAdminApi googleAdminApi = scope.ServiceProvider.GetRequiredService<IGoogleAdminApi>();
        IOUGroupRelationsRepository oUGroupRelationsRepository = scope.ServiceProvider.GetRequiredService<IOUGroupRelationsRepository>();
        IJobsRepository jobsRepository = scope.ServiceProvider.GetRequiredService<IJobsRepository>();
        ILogStore logStore = scope.ServiceProvider.GetRequiredService<ILogStore>();

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
    }
}