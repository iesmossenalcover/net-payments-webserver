using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.People.Queries;

# region ViewModels
public record GetPeopleVm(IEnumerable<PersonRowVm> People);
#endregion

public record GetPeopleQuery(string Query) : IRequest<IEnumerable<PersonRowVm>>;

public class GetPeopleQueryHandler : IRequestHandler<GetPeopleQuery, IEnumerable<PersonRowVm>>
{
    # region IOC
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;

    public GetPeopleQueryHandler(IPersonGroupCourseRepository personGroupCourseRepository)
    {
        _personGroupCourseRepository = personGroupCourseRepository;
    }
    #endregion

    public async Task<IEnumerable<PersonRowVm>> Handle(GetPeopleQuery request, CancellationToken ct)
    {
        IEnumerable<PersonGroupCourse> ppgc = await _personGroupCourseRepository.FilterPeople(new FilterPeople(request.Query), 50, ct);
        return ppgc.GroupBy(x => x.Person).Select(x =>
        {
            PersonGroupCourse pgc = x.OrderByDescending(x => x.Course.StartDate).First();
            return new PersonRowVm(x.Key.Id, x.Key.DocumentId, x.Key.Name, x.Key.LastName, pgc.Course.Active ? pgc.GroupId : null, pgc.Course.Active ? pgc.Group.Name : null, pgc.Amipa, x.Key.AcademicRecordNumber);
        });
    }
}
