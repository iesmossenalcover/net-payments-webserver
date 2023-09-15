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
    private readonly IServiceProvider _serviceProvider;
    private readonly string[] excludeEmails;

    public MovePeopleGoogleWorkspaceCommandHandler(IJobsRepository jobsRepository, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _jobsRepository = jobsRepository;
        excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
    }
    #endregion

    public async Task<Response<MovePeopleGoogleWorkspaceCommandVm>> Handle(MovePeopleGoogleWorkspaceCommand request, CancellationToken ct)
    {
        // Start Task and try to save task
        var task = new Job()
        {
            Status = JobStatus.RUNNING,
            Start = DateTimeOffset.UtcNow,
            Type = JobType.MOVE_PEOPLE_GOOGLE_WORKSPACE,
        };

        var queuedTask = await _jobsRepository.AtomicInsertJobAsync(task);
        if (!queuedTask)
        {
            return Response<MovePeopleGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "Ja hi ha una tasca del mateix tipus iniciada.");
        }

        _ = Task.Run(async () =>
        {
            ct = CancellationToken.None;
            using var scope = _serviceProvider.CreateAsyncScope();
            IGoogleAdminApi googleAdminApi = scope.ServiceProvider.GetRequiredService<IGoogleAdminApi>();
            IOUGroupRelationsRepository oUGroupRelationsRepository = scope.ServiceProvider.GetRequiredService<IOUGroupRelationsRepository>();
            IJobsRepository jobsRepository = scope.ServiceProvider.GetRequiredService<IJobsRepository>();
            ILogStore logStore = scope.ServiceProvider.GetRequiredService<ILogStore>();

            var log = new Log();
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

            log.Add("Fi tasca");
            var logStoreInfo = await logStore.Save(log);
            task = await jobsRepository.GetByIdAsync(task.Id, ct);
            if (task == null) return;
            task.Log = logStoreInfo;
            task.End = DateTimeOffset.UtcNow;
            task.Status = JobStatus.FINISHED;
            await _jobsRepository.UpdateAsync(task, ct);
        });

        return Response<MovePeopleGoogleWorkspaceCommandVm>.Ok(new MovePeopleGoogleWorkspaceCommandVm(true));
    }
}