using Application.Common;
using Application.Common.Models;
using Domain.Services;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using MediatR;

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
    private readonly IOUGroupRelationsRepository _oUGroupRelationsRepository;
    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly IJobsRepository _tasksRepository;
    private readonly ILogStore _logStore;
    private readonly string[] excludeEmails;

    public MovePeopleGoogleWorkspaceCommandHandler(IOUGroupRelationsRepository oUGroupRelationsRepository, IGoogleAdminApi googleAdminApi, ILogStore logStore, IJobsRepository tasksRepository, IConfiguration configuration)
    {
        _googleAdminApi = googleAdminApi;
        _oUGroupRelationsRepository = oUGroupRelationsRepository;
        _tasksRepository = tasksRepository;
        _logStore = logStore;
        excludeEmails = configuration.GetValue<string>("GoogleApiExcludeAccounts")?.Split(" ") ?? throw new Exception("GoogleApiExcludeAccounts");
    }
    #endregion

    public async Task<Response<MovePeopleGoogleWorkspaceCommandVm>> Handle(MovePeopleGoogleWorkspaceCommand request, CancellationToken ct)
    {
        // Start Task and try to save task
        var task = new Domain.Entities.Tasks.Job()
        {
            Status = Domain.Entities.Tasks.JobStatus.RUNNING,
            Start = DateTimeOffset.UtcNow,
            Type = Domain.Entities.Tasks.JobType.MOVE_PEOPLE_GOOGLE_WORKSPACE,
        };

        var queuedTask = await _tasksRepository.AtomicInsertTaskAsync(task);
        if (!queuedTask)
        {
            return Response<MovePeopleGoogleWorkspaceCommandVm>.Error(ResponseCode.BadRequest, "Ja hi ha una tasca del mateix tipus en marxa.");
        }

        _ = Task.Run(async () =>
        {
            ct = CancellationToken.None;

            var log = new Domain.Entities.Tasks.Log();
            log.Add("Inici tasca");

            IEnumerable<UoGroupRelation> ouRelations = await _oUGroupRelationsRepository.GetAllAsync(ct);
            foreach (var ou in ouRelations)
            {
                GoogleApiResult<IEnumerable<string>> usersResult = await _googleAdminApi.GetAllUsers(ou.ActiveOU);
                if (!usersResult.Success || usersResult.Data == null)
                {
                    log.Add($"Error recuperant usuaris OU: {ou.GroupMail}");
                    continue;
                }

                foreach (var user in usersResult.Data)
                {
                    // IMPORTANT: Exclude members
                    if (excludeEmails.Contains(user)) continue;

                    var result = await _googleAdminApi.MoveUserToOU(user, ou.OldOU);
                    if (!result.Success)
                    {
                        log.Add($"Error recuperant usuaris OU: {ou.GroupMail} USER: {user}");
                    }
                }
            }

            log.Add("Fi tasca");
            var logStoreInfo = await _logStore.Save(log);
            task.Log = logStoreInfo;
            await _tasksRepository.UpdateAsync(task, ct);
        });

        return Response<MovePeopleGoogleWorkspaceCommandVm>.Ok(new MovePeopleGoogleWorkspaceCommandVm(true));
    }
}