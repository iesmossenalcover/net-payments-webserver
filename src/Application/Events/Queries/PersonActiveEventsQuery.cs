using Application.Common;
using Application.Common.Services;
using Domain.Entities.Configuration;
using Domain.Entities.Events;
using Domain.Entities.People;
using MediatR;

namespace Application.Events.Queries;

# region ViewModels
public record PersonSummaryVm(string DocumentId, string FullName, bool Enrolled, string? EnrollmentSubjectsInfo, string? GroupDescription);
public record PublicEventVm(string Code, string Name, DateTimeOffset Date, decimal Price, string CurrencySymbol, bool Selectable);
public record PersonActiveEventsVm(IEnumerable<PublicEventVm> Events, PersonSummaryVm person);
#endregion

#region Query
public record PersonActiveEventsQuery(string DocumentId) : IRequest<Response<PersonActiveEventsVm>>;
#endregion

public class PersonActiveEventsQueryHandler : IRequestHandler<PersonActiveEventsQuery, Response<PersonActiveEventsVm>>
{
    #region  IOC
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _peopleGroupCourseRepository;
    private readonly ICoursesRepository _coursesRepository;
    private readonly IAppConfigRepository _appConfigRepository;

    public PersonActiveEventsQueryHandler(IEventsPeopleRespository eventsPeopleRepository, IPersonGroupCourseRepository peopleGroupCourseRepository, ICoursesRepository coursesRepository, IAppConfigRepository appConfigRepository)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _peopleGroupCourseRepository = peopleGroupCourseRepository;
        _coursesRepository = coursesRepository;
        _appConfigRepository = appConfigRepository;
    }

    #endregion

    public async Task<Response<PersonActiveEventsVm>> Handle(PersonActiveEventsQuery request, CancellationToken ct)
    {
        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        PersonGroupCourse? pgc = await _peopleGroupCourseRepository.GetCoursePersonGroupByDocumentId(request.DocumentId.Trim(), course.Id, ct);

        if (pgc == null)
        {
            return Response<PersonActiveEventsVm>.Error(ResponseCode.NotFound, "No s'ha trobat cap persona amb aquest document al curs actual.");
        }

        Person person = pgc.Person;
        IEnumerable<Domain.Entities.Events.EventPerson> personEvents = await _eventsPeopleRepository.GetAllByPersonAndCourse(person.Id, course.Id, ct);
        personEvents = personEvents.Where(x => x.CanBePaid);

        AppConfig config = await _appConfigRepository.GetAsync(ct);

        // TODO: Decide with events are selectable, for the moment all are selectable
        IEnumerable<PublicEventVm> eventsVm = personEvents.Select(x => ToPublicEventVm(x, pgc));

        return Response<PersonActiveEventsVm>.Ok(
            new PersonActiveEventsVm(eventsVm, ToPersonSummaryVm(person, pgc, config))
        );
    }

    public static PublicEventVm ToPublicEventVm(EventPerson x, PersonGroupCourse pgc)
    {
        // , pgc.Enrolled, pgc.Enrolled ? pgc.SubjectsInfo : null
        return new PublicEventVm(
            x.Event.Code, x.Event.Name, x.Event.Date, pgc.PriceForEvent(x.Event), "€", true
        );
    }

    public static PersonSummaryVm ToPersonSummaryVm(Person person, PersonGroupCourse pgc, AppConfig config)
    {
        bool displayEnrollment = pgc.Enrolled && config.DisplayEnrollment;
        return new PersonSummaryVm(person.DocumentId, person.FullName, displayEnrollment, displayEnrollment ? pgc.SubjectsInfo : null, displayEnrollment ? pgc.Group.Description : null);
    }
}
