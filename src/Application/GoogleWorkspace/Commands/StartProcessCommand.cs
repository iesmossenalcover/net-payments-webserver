using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using MediatR;
using Domain.Entities.Jobs;
using Domain.ValueObjects;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record StartProcessCommand(JobType Type) : IRequest<Response<StartProcessCommandVm>>;

// Validator for the model

// Optionally define a view model
public record StartProcessCommandVm(bool ok);

// Handler
public class StartProcessCommandHandler : IRequestHandler<StartProcessCommand, Response<StartProcessCommandVm>>
{
    #region props
    private readonly IJobsRepository _jobsRepository;
    private readonly ProcessRunner _processRunner;
    private readonly IServiceProvider _serviceProvider;

    public StartProcessCommandHandler(IJobsRepository jobsRepository, ProcessRunner processRunner, IServiceProvider serviceProvider)
    {
        _jobsRepository = jobsRepository;
        _processRunner = processRunner;
        _serviceProvider = serviceProvider;
    }
    #endregion

    public async Task<Response<StartProcessCommandVm>> Handle(StartProcessCommand request, CancellationToken ct)
    {
        IProcess? process = null;

        switch (request.Type)
        {
            case JobType.SUSPEND_GOOGLE_WORKSPACE:
                process = new SuspendGoogleWorkspaceProcess();
                break;
            case JobType.MOVE_PEOPLE_GOOGLE_WORKSPACE:
                var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
                var excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
                process = new MovePeopleGoogleWorkspaceProcess(excludeEmails);
                break;
            default:
                break;
        }

        if (process == null)
        {
            return Response<StartProcessCommandVm>.Error(ResponseCode.BadRequest, "Tipus de tasca no valid");
        }

        // Queue the job if it is possible.
        var job = new Job()
        {
            Status = JobStatus.PENDING,
            Start = DateTimeOffset.UtcNow,
            Type = request.Type,
        };

        var queuedTask = await _jobsRepository.AtomicInsertJobAsync(job);
        if (!queuedTask)
        {
            return Response<StartProcessCommandVm>.Error(ResponseCode.BadRequest, "Ja hi ha una tasca del mateix tipus iniciada.");
        }

        _processRunner.Start(process, job.Id);
        return Response<StartProcessCommandVm>.Ok(new StartProcessCommandVm(true));
    }
}