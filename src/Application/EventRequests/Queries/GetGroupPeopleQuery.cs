using Domain.Entities.People;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Queries;

public record GroupPersonVm(long Id, string Name);

/// <summary>
/// Alumnes d'un grup al curs actual, perquè el professor pugui triar-ne només alguns a la sol·licitud.
/// Ho pot veure qualsevol professor (reader), per això només retorna l'id i el nom.
/// </summary>
public record GetGroupPeopleQuery(long GroupId) : IRequest<IEnumerable<GroupPersonVm>>;

public class GetGroupPeopleQueryHandler : IRequestHandler<GetGroupPeopleQuery, IEnumerable<GroupPersonVm>>
{
    #region IOC
    private readonly ICoursesRepository _coursesRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;

    public GetGroupPeopleQueryHandler(ICoursesRepository coursesRepository, IPersonGroupCourseRepository personGroupCourseRepository)
    {
        _coursesRepository = coursesRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
    }
    #endregion

    public async Task<IEnumerable<GroupPersonVm>> Handle(GetGroupPeopleQuery request, CancellationToken ct)
    {
        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        var members = await _personGroupCourseRepository.GetPeopleGroupByGroupIdAndCourseIdAsync(course.Id, request.GroupId, ct);

        return members
            .Select(x => x.Person)
            .OrderBy(x => x.Surname1).ThenBy(x => x.Surname2).ThenBy(x => x.Name)
            .Select(x => new GroupPersonVm(x.Id, x.FormalFullName))
            .ToList();
    }
}
