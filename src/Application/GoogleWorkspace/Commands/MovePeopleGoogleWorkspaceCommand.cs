using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using MediatR;
using Domain.Entities.Jobs;
using Domain.ValueObjects;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record MovePeopleGoogleWorkspaceCommand() : IRequest<Response<MovePeopleGoogleWorkspaceCommandVm>>;

// Validator for the model

// Optionally define a view model
public record MovePeopleGoogleWorkspaceCommandVm(bool ok);

// Handler
public class MovePeopleGoogleWorkspaceCommandHandler : IRequestHandler<MovePeopleGoogleWorkspaceCommand, Response<MovePeopleGoogleWorkspaceCommandVm>>
{
    #region props
    private readonly IJobsRepository _jobsRepository;
    private readonly ProcessRunner _processRunner;
    private readonly string[] excludeEmails;

    public MovePeopleGoogleWorkspaceCommandHandler(IJobsRepository jobsRepository, IConfiguration configuration, ProcessRunner processRunner)
    {
        _jobsRepository = jobsRepository;
        _processRunner = processRunner;
        excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
    }
    #endregion

    public async Task<Response<MovePeopleGoogleWorkspaceCommandVm>> Handle(MovePeopleGoogleWorkspaceCommand request, CancellationToken ct)
    {
        // Queue the job if it is possible.
        var job = new Job()
        {
            Status = JobStatus.PENDING,
            Start = DateTimeOffset.UtcNow,
            Type = JobType.MOVE_PEOPLE_GOOGLE_WORKSPACE,
        };

        var queuedTask = await _jobsRepository.AtomicInsertJobAsync(job);
        if (!queuedTask)
        {
            return Response<MovePeopleGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "Ja hi ha una tasca del mateix tipus iniciada.");
        }

        var process = new MovePeopleGoogleWorkspaceProcess(excludeEmails);
        _processRunner.Start(process, job.Id);

        return Response<MovePeopleGoogleWorkspaceCommandVm>.Ok(new MovePeopleGoogleWorkspaceCommandVm(true));
    }
}

public class MovePeopleGoogleWorkspaceProcess : IProcess
{
    private readonly string[] excludeEmails;

    public MovePeopleGoogleWorkspaceProcess(string[] excludeEmails)
    {
        this.excludeEmails = excludeEmails;
    }

    public async Task Run(IServiceScopeFactory serviceProvider, Log log, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        IGoogleAdminApi googleAdminApi = scope.ServiceProvider.GetRequiredService<IGoogleAdminApi>();
        IOUGroupRelationsRepository oUGroupRelationsRepository = scope.ServiceProvider.GetRequiredService<IOUGroupRelationsRepository>();
        IJobsRepository jobsRepository = scope.ServiceProvider.GetRequiredService<IJobsRepository>();
        ILogStore logStore = scope.ServiceProvider.GetRequiredService<ILogStore>();

        log.Add("Inici tasca");

        IEnumerable<UoGroupRelation> ouRelations = await oUGroupRelationsRepository.GetAllAsync(ct);
        foreach (var ou in ouRelations)
        {
            GoogleApiResult<IEnumerable<string>> usersResult = await googleAdminApi.GetAllUsers(ou.ActiveOU);
            if (!usersResult.Success || usersResult.Data == null)
            {
                log.Add($"Error recuperant usuaris OU: {ou.GroupMail}");
                continue;
            }

            foreach (var user in usersResult.Data)
            {
                // IMPORTANT: Exclude members
                if (excludeEmails.Contains(user)) continue;

                var result = await googleAdminApi.MoveUserToOU(user, ou.OldOU);
                if (!result.Success)
                {
                    log.Add($"Error recuperant usuaris OU: {ou.GroupMail} USER: {user}");
                }
            }
        }
    }
}