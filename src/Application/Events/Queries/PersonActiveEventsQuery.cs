using Application.Common;
using Domain.Services;
using Domain.Entities.Configuration;
using Domain.Entities.Events;
using Domain.Entities.People;
using MediatR;

namespace Application.Events.Queries;

# region ViewModels
public record PersonSummaryVm(string DocumentId, string FullName, bool Enrolled, string? EnrollmentSubjectsInfo, string? GroupDescription);
public record PublicEventVm(string Code, string Name, DateTimeOffset Date, decimal Price, string CurrencySymbol, bool Selectable, bool DisplayQuantitySelector, uint MaxQuantity, string? MissingAuthorization);
public record MissingAuthorizationsVm(bool Walking, bool Transport, string ContactPhone);
public record PersonActiveEventsVm(IEnumerable<PublicEventVm> Events, PersonSummaryVm Person, MissingAuthorizationsVm? MissingAuthorizations);
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

    private readonly string contactPhoneNumber;

    public PersonActiveEventsQueryHandler(IEventsPeopleRespository eventsPeopleRepository, IPersonGroupCourseRepository peopleGroupCourseRepository, ICoursesRepository coursesRepository, IAppConfigRepository appConfigRepository, IConfiguration configuration)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _peopleGroupCourseRepository = peopleGroupCourseRepository;
        _coursesRepository = coursesRepository;
        _appConfigRepository = appConfigRepository;
        contactPhoneNumber = configuration.GetValue<string>("ContactPhoneNumber") ?? throw new Exception("ContactPhoneNumber");

    }

    #endregion

    public async Task<Response<PersonActiveEventsVm>> Handle(PersonActiveEventsQuery request, CancellationToken ct)
    {
        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        PersonGroupCourse? pgc = await _peopleGroupCourseRepository.GetCoursePersonGroupByDocumentId(request.DocumentId.Trim(), course.Id, ct);
        AppConfig config = await _appConfigRepository.GetAsync(ct);

        if (pgc == null)
        {
            string message = "No s'ha trobat cap persona amb aquest document al curs actual.";
            if(config.DisplayEnrollment){
                message = $"Document no trobat. Contacta amb l'oficina de l'institut: {contactPhoneNumber}";
            }
            return Response<PersonActiveEventsVm>.Error(ResponseCode.NotFound, message);
        }

        Person person = pgc.Person;
        IEnumerable<EventPerson> allPersonEvents = await _eventsPeopleRepository.GetAllByPersonAndCourse(person.Id, course.Id, ct);
        IEnumerable<EventPerson> personEvents = allPersonEvents.Where(x => x.CanBePaid);


        IEnumerable<PublicEventVm> eventsVm = personEvents.Select(x => ToPublicEventVm(x, pgc));

        return Response<PersonActiveEventsVm>.Ok(
            new PersonActiveEventsVm(eventsVm, ToPersonSummaryVm(person, pgc, config), GetMissingAuthorizations(allPersonEvents, pgc))
        );
    }

    // Warn about missing authorizations while the person has events of the current course (paid or not) that require them.
    private MissingAuthorizationsVm? GetMissingAuthorizations(IEnumerable<EventPerson> personEvents, PersonGroupCourse pgc)
    {
        IEnumerable<Event> unauthorized = personEvents
            .Select(x => x.Event)
            .Where(x => !pgc.IsAuthorizedFor(x));

        bool walking = unauthorized.Any(x => x.Type == EventType.Walking);
        bool transport = unauthorized.Any(x => x.Type == EventType.Transport);

        if (!walking && !transport) return null;
        return new MissingAuthorizationsVm(walking, transport, contactPhoneNumber);
    }

    public static PublicEventVm ToPublicEventVm(EventPerson x, PersonGroupCourse pgc)
    {
        // The event can still be paid, but the family is told which authorization is missing.
        string? missingAuthorization = pgc.IsAuthorizedFor(x.Event) ? null : PersonGroupCourse.MissingAuthorizationMessage(x.Event);
        return new PublicEventVm(x.Event.Code, x.Event.Name, x.Event.Date, pgc.PriceForEvent(x.Event), "€", true, x.Event.MaxQuantity > 1, x.Event.MaxQuantity, missingAuthorization);
    }

    public static PersonSummaryVm ToPersonSummaryVm(Person person, PersonGroupCourse pgc, AppConfig config)
    {
        bool displayEnrollment = pgc.Enrolled && config.DisplayEnrollment;
        return new PersonSummaryVm(person.DocumentId, person.FullName, displayEnrollment, displayEnrollment ? pgc.SubjectsInfo : null, displayEnrollment ? pgc.Group.Description : null);
    }
}
