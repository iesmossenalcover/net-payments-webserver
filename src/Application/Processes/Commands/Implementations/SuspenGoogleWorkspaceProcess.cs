using Application.Common.Models;
using Domain.Entities.GoogleApi;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.Processes.Commands.Implementations;

public class SuspendGoogleWorkspaceProcess : IProcess
{
    public async Task Run(IServiceScopeFactory serviceProvider, Log log, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        IGoogleAdminApi googleAdminApi = scope.ServiceProvider.GetRequiredService<IGoogleAdminApi>();
        IOUGroupRelationsRepository oUGroupRelationsRepository = scope.ServiceProvider.GetRequiredService<IOUGroupRelationsRepository>();

        IEnumerable<OuGroupRelation> ouRelations = await oUGroupRelationsRepository.GetAllAsync(ct);
        IEnumerable<string> pendings = ouRelations.Select(x => x.OldOU).Distinct();


        int total = 0;
        foreach (var ou in pendings)
        {
            // Una OU buida a la configuració faria una consulta sense filtre d'OU.
            if (string.IsNullOrWhiteSpace(ou))
            {
                log.Add("OU buida a la configuració - [Error] no es pot processar");
                continue;
            }

            GoogleApiResult<int> result = await googleAdminApi.SetSuspendByOU(ou, true, false);
            if (!result.Success)
            {
                log.Add($"OU {ou} - [Error] {result.ErrorMessage ?? "No s'ha pogut processar"}");
            }
            else
            {
                total += result.Data;
                log.Add($"OU: {ou} - [OK] Usuaris suspesos: {result.Data}");
            }
        }

        log.Add($"Total d'usuaris suspesos: {total}");
    }
}