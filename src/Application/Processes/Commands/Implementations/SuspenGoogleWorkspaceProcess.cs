using Application.Common.Models;
using Domain.Entities.GoogleApi;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.Processes.Commands.Implementations;

public class SuspendGoogleWorkspaceProcess : IProcess
{
    public async Task Run(IServiceScopeFactory serviceProvider, Log log, CancellationToken ct)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        IGoogleAdminApi googleAdminApi = scope.ServiceProvider.GetRequiredService<IGoogleAdminApi>();
        IOUGroupRelationsRepository oUGroupRelationsRepository = scope.ServiceProvider.GetRequiredService<IOUGroupRelationsRepository>();

        IEnumerable<OuGroupRelation> ouRelations = await oUGroupRelationsRepository.GetAllAsync(true, ct);
        IEnumerable<string> pendingOus = ouRelations.Select(x => x.OldOU).Distinct();


        foreach (string ou in pendingOus)
        {
            GoogleApiResult<bool> result = await googleAdminApi.SetSuspendByOU(ou, true, false);
            log.Add(!result.Success
                ? $"OU {ou} - [Error] {result.ErrorMessage ?? @"No s'ha pogut processar"}"
                : $"OU: {ou} - [OK]");
        }
    }
}