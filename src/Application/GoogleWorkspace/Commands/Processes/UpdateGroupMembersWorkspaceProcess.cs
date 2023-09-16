using Application.Common.Models;
using Domain.Entities.GoogleApi;
using Domain.Entities.People;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.GoogleWorkspace.Commands.Processes;

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
        IEnumerable<UoGroupRelation> ouRelations = await oUGroupRelationsRepository.GetAllAsync(ct);

        if (!ouRelations.Any())
        {
            log.Add("No hi ha unitats organitzatives configurades");
        }

        foreach (var ou in ouRelations)
        {
            GoogleApiResult<bool> groupResult = await googleAdminApi.ClearGroupMembers(ou.GroupMail);
            if (!groupResult.Success)
            {
                log.Add($"OU: {ou} - Error buidant membres. Missatge: {groupResult.ErrorMessage ?? string.Empty}");
                continue;
            }

            IEnumerable<PersonGroupCourse> pgcs = await personGroupCourseRepository.GetPeopleGroupByGroupIdAndCourseIdAsync(course.Id, ou.GroupId, ct);

            foreach (var pgc in pgcs)
            {
                Person p = pgc.Person;

                if (!string.IsNullOrEmpty(p.ContactMail))
                {
                    var result = await googleAdminApi.AddUserToGroup(p.ContactMail, ou.GroupMail);
                    if (!result.Success)
                    {
                        log.Add($"OU: {ou} User: {p.ContactMail} Group: {ou.GroupMail} - Error afegint usuari a grup. Missatge: {result.ErrorMessage ?? string.Empty}");
                        continue;
                    }
                }
            }

        }
    }
}