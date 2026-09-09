using Application.Common.Models;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.Processes.Commands.Implementations;

public class UpdateGroupMembersWorkspaceProcess : IProcess
{
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
            foreach (var pgc in pgcs)
            {
                Person p = pgc.Person;

                if (string.IsNullOrEmpty(p.ContactMail))
                {
                    withoutMail++;
                    continue;
                }

                var result = await googleAdminApi.AddUserToGroup(p.ContactMail, ou.GroupMail);
                if (!result.Success)
                {
                    log.Add($"OU: {ou.GroupMail} User: {p.ContactMail} Group: {ou.GroupMail} - Error afegint usuari a grup. Missatge: {result.ErrorMessage ?? string.Empty}");
                    continue;
                }

                added++;
            }

            log.Add($"OU: {ou.GroupMail} - Membres afegits: {added}/{pgcs.Count} (sense correu de contacte: {withoutMail})");

        }
    }
}