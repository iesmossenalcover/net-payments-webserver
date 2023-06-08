using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using Domain.Entities.People;
using Infrastructure.Repos;
using MediatR;

namespace Application.Events.Queries;

# region ViewModels
public record EventPersonVm(long Id, string DocumentId, string FullName, long? AcademicRecordNumber, bool inEvent);
public record EventPeopleGroupVm(long Id, string Name, int Order, IList<EventPersonVm> People);
public record EventPeopleVm(long Id, string Code, string Name, DateTimeOffset Date, IEnumerable<EventPeopleGroupVm> PeopleGroups);
#endregion

#region Query
public record EventPeopleQuery(string Code) : IRequest<Response<EventPeopleVm>>;
#endregion

public class EventPeopleQueryHandler : IRequestHandler<EventPeopleQuery, Response<EventPeopleVm>>
{
    #region  IOC
    private readonly IEventsRespository _eventsRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly ICoursesRepository _coursesRepository;

    public EventPeopleQueryHandler(IEventsRespository eventsRepository, IEventsPeopleRespository eventsPeopleRepository, IPersonGroupCourseRepository personGroupCourseRepository, ICoursesRepository coursesRepository)
    {
        _eventsRepository = eventsRepository;
        _eventsPeopleRepository = eventsPeopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _coursesRepository = coursesRepository;
    }

    #endregion

    public async Task<Response<EventPeopleVm>> Handle(EventPeopleQuery request, CancellationToken ct)
    {
        Event? e = await _eventsRepository.GetEventByCodeAsync(request.Code, ct);
        if (e == null) return Response<EventPeopleVm>.Error(ResponseCode.NotFound, "Esdeveniment no trobat");

        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);
        IQueryable<PersonGroupCourse> people = 
                            _personGroupCourseRepository.GetPersonGroupCourseByCourseAsync(course.Id, ct)
                            .OrderBy(x => x.Person.Name)
                            .ThenBy(x => x.Person.Surname1)
                            .ThenBy(x => x.Person.Surname2);
                            
        Dictionary<long, Person> eventPeople = (await _eventsPeopleRepository.GetAllByEventIdAsync(e.Id, ct)).ToDictionary(x => x.PersonId, x => x.Person);

        Dictionary<long, EventPeopleGroupVm> groups = new Dictionary<long, EventPeopleGroupVm>();
        foreach (var pgc in people)
        {
            EventPeopleGroupVm group = default!;
            if (!groups.ContainsKey(pgc.GroupId))
            {
                group = new EventPeopleGroupVm(pgc.GroupId, pgc.Group.Name, pgc.Group.Order, new List<EventPersonVm>());
                groups.Add(pgc.GroupId, group);
            }
            else
            {
                group = groups[pgc.GroupId];
            }

            EventPersonVm p = new EventPersonVm(
                pgc.Person.Id,
                pgc.Person.DocumentId,
                pgc.Person.FullName,
                pgc.Person.AcademicRecordNumber,
                eventPeople.ContainsKey(pgc.PersonId)
            );
            group.People.Add(p);
        }

        return Response<EventPeopleVm>.Ok(
            new EventPeopleVm(e.Id, e.Code, e.Name, e.Date, groups.Select(x => x.Value).OrderBy(x => x.Name))
        );
    }
}
