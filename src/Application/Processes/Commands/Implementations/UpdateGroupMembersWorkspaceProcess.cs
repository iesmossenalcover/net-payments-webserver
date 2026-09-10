using Application.Common.Models;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.Processes.Commands.Implementations;

public class UpdateGroupMembersWorkspaceProcess : IProcess
{
    // Altes simultànies contra l'API de Google dins d'un mateix grup.
    // Els grups es processen un darrere l'altre per no disparar el nombre total de
    // peticions en paral·lel i per mantenir l'ordre del registre.
    private const int ADD_MEMBER_PARALLELISM = 5;

    public async Task Run(IServiceScopeFactory serviceProvider, Log log, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateAsyncScope();
        IGoogleAdminApi googleAdminApi = scope.ServiceProvider.GetRequiredService<IGoogleAdminApi>();
        IOUGroupRelationsRepository oUGroupRelationsRepository = scope.ServiceProvider.GetRequiredService<IOUGroupRelationsRepository>();
        ICoursesRepository coursesRepository = scope.ServiceProvider.GetRequiredService<ICoursesRepository>();
        IPersonGroupCourseRepository personGroupCourseRepository = scope.ServiceProvider.GetRequiredService<IPersonGroupCourseRepository>();

        Course course = await coursesRepository.GetCurrentCoursAsync(ct);
        IEnumerable<OuGroupRelation> ouRelations = await oUGroupRelationsRepository.GetAllAsync(ct);

        if (!ouRelations.Any())
        {
            log.Add("No hi ha unitats organitzatives configurades");
        }

        foreach (var ou in ouRelations)
        {
            GoogleApiResult<int> groupResult = await googleAdminApi.ClearGroupMembers(ou.GroupMail);
            if (!groupResult.Success)
            {
                log.Add($"OU: {ou.GroupMail} - Error buidant membres. Missatge: {groupResult.ErrorMessage ?? string.Empty}");
                continue;
            }

            log.Add($"OU: {ou.GroupMail} - Membres esborrats: {groupResult.Data}");

            List<PersonGroupCourse> pgcs = (await personGroupCourseRepository.GetPeopleGroupByGroupIdAndCourseIdAsync(course.Id, ou.GroupId, ct)).ToList();

            int added = 0;
            int withoutMail = 0;

            // Els errors es desen per posició per escriure'ls al registre en el mateix
            // ordre que abans, encara que les altes acabin desordenades.
            string?[] addErrors = new string?[pgcs.Count];

            await Parallel.ForEachAsync(
                Enumerable.Range(0, pgcs.Count),
                new ParallelOptions { MaxDegreeOfParallelism = ADD_MEMBER_PARALLELISM },
                async (index, _) =>
                {
                    Person p = pgcs[index].Person;

                    if (string.IsNullOrEmpty(p.ContactMail))
                    {
                        Interlocked.Increment(ref withoutMail);
                        return;
                    }

                    var result = await googleAdminApi.AddUserToGroup(p.ContactMail, ou.GroupMail);
                    if (!result.Success)
                    {
                        addErrors[index] = $"OU: {ou.GroupMail} User: {p.ContactMail} Group: {ou.GroupMail} - Error afegint usuari a grup. Missatge: {result.ErrorMessage ?? string.Empty}";
                        return;
                    }

                    Interlocked.Increment(ref added);
                });

            foreach (string? addError in addErrors)
            {
                if (addError != null)
                {
                    log.Add(addError);
                }
            }

            log.Add($"OU: {ou.GroupMail} - Membres afegits: {added}/{pgcs.Count} (sense correu de contacte: {withoutMail})");

        }
    }
}