using Application.Common.Models;
using Domain.Entities.GoogleApi;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.GoogleWorkspace.Commands.Processes;

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