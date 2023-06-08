using Application.Common.Services;
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
        return ppgc.Select(x => new PersonRowVm(x.PersonId, x.Person.DocumentId, x.Person.Name, x.Person.Surname1, x.Person.Surname2, x.GroupId, x.Group.Name, x.Amipa, x.Person.AcademicRecordNumber));
    }
}
