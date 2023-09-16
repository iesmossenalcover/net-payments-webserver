using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using MediatR;
using Domain.Entities.Jobs;
using Domain.ValueObjects;
using Application.GoogleWorkspace.Commands.Processes;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record StartProcessCommand(JobType Type) : IRequest<Response<StartProcessCommandVm>>;

// Validator for the model

// Optionally define a view model
public record StartProcessCommandVm(bool Ok);

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
        IProcess? process = InstantiateProcess(request.Type);
        if (process == null) return Response<StartProcessCommandVm>.Error(ResponseCode.BadRequest, "Tasca no vàlida");

        var job = new Job()
        {
            Status = JobStatus.PENDING,
            Start = DateTimeOffset.UtcNow,
            Type = request.Type,
        };
        
        /*
            Right now it is hardcoded that only one task of same type can be running.
            IProcess could implment a method to determine if a process can be spawned or not.
            This way each processs could have its own logic.
        */
        var queuedTask = await _jobsRepository.AtomicInsertJobAsync(job);
        if (!queuedTask)
        {
            return Response<StartProcessCommandVm>.Error(ResponseCode.BadRequest, "Ja hi ha una tasca del mateix tipus iniciada.");
        }

        _processRunner.Start(process, job.Id);

        return Response<StartProcessCommandVm>.Ok(new StartProcessCommandVm(true));
    }

    private IProcess? InstantiateProcess(JobType type)
    {
        switch (type)
        {
            case JobType.SUSPEND_GOOGLE_WORKSPACE:
                return new SuspendGoogleWorkspaceProcess();

            case JobType.MOVE_PEOPLE_GOOGLE_WORKSPACE:
                var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
                var excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
                return new MovePeopleGoogleWorkspaceProcess(excludeEmails);

            case JobType.UPDATE_GROUP_MEMBERS_WORKSPACE:
                return new UpdateGroupMembersWorkspaceProcess();

            default:
                return null;
        }
    }
}