using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using Infrastructure.Repos;
using MediatR;

namespace Application.Events.Queries;

# region ViewModels
public record PersonVm(string DocumentId, string FullName);
public record PublicEventVm(string Code, string Name, decimal Price, bool selectable);
public record PersonActiveEventsVm(IEnumerable<PublicEventVm> Events, PersonVm person);
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

    public PersonActiveEventsQueryHandler(IEventsPeopleRespository eventsPeopleRepository, IPersonGroupCourseRepository peopleGroupCourseRepository, ICoursesRepository coursesRepository)
    {
        _eventsPeopleRepository = eventsPeopleRepository;
        _peopleGroupCourseRepository = peopleGroupCourseRepository;
        _coursesRepository = coursesRepository;
    }
    #endregion

    public async Task<Response<PersonActiveEventsVm>> Handle(PersonActiveEventsQuery request, CancellationToken ct)
    {
        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        PersonGroupCourse? pgc = await _peopleGroupCourseRepository.GetCoursePersonGroupBy(request.DocumentId, course.Id, ct);
        
        if (pgc == null)
        {
            return Response<PersonActiveEventsVm>.Error(ResponseCode.NotFound, "No s'ha trobat cap persona amb aquest document al curs actual.");
        }

        Person person = pgc.Person;
        IEnumerable<EventPerson> personEvents = await _eventsPeopleRepository.GetAllByPersonAndCourse(person.Id, course.Id, ct);
        personEvents = personEvents.Where(x => x.Event.IsActive && !x.Paid);

        // TODO: Decide with events are selectable, for the moment all are selectable
        IEnumerable<PublicEventVm> eventsVm = personEvents.Select(x => new PublicEventVm(x.Event.Code, x.Event.Name, pgc.PriceForEvent(x.Event), true));

        return Response<PersonActiveEventsVm>.Ok(
            new PersonActiveEventsVm(eventsVm, new PersonVm(person.DocumentId, $"{person.Name} {person.Surname1} {person.Surname2}"))
        );
    }
}
