using Application.Common.Models;
using Domain.Entities.GoogleApi;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.GoogleWorkspace.Commands.Processes;

public class SuspendGoogleWorkspaceProcess : IProcess
{
    public async Task Run(IServiceScopeFactory serviceProvider, Log log, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        IGoogleAdminApi googleAdminApi = scope.ServiceProvider.GetRequiredService<IGoogleAdminApi>();
        IOUGroupRelationsRepository oUGroupRelationsRepository = scope.ServiceProvider.GetRequiredService<IOUGroupRelationsRepository>();

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