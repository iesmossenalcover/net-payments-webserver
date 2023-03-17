using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using FluentValidation;
using MediatR;
using System.Text;
using Domain.Entities.People;

namespace Application.Events.Commands;

public record SetPeopleEventCommand : IRequest<Response<bool?>>
{
    public string EventCode { get; set; } = default!;
    public IEnumerable<long> PeopleIds { get; set; } = Enumerable.Empty<long>();
}

public class SetPeopleEventCommandValidator : AbstractValidator<SetPeopleEventCommand>
{
    public SetPeopleEventCommandValidator()
    {
    }
}

public class SetPeopleEventCommandHandler : IRequestHandler<SetPeopleEventCommand, Response<bool?>>
{
    #region IOC
    private readonly IPeopleRepository _peopleRepository;
    private readonly IEventsRespository _eventsRespository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;

    public SetPeopleEventCommandHandler(IPeopleRepository peopleRepository, IEventsRespository eventsRespository, IEventsPeopleRespository eventsPeopleRepository)
    {
        _peopleRepository = peopleRepository;
        _eventsRespository = eventsRespository;
        _eventsPeopleRepository = eventsPeopleRepository;
    }
    #endregion

    public async Task<Response<bool?>> Handle(SetPeopleEventCommand request, CancellationToken ct)
    {
        Event? e = await _eventsRespository.GetEventByCodeAsync(request.EventCode, ct);
        if (e == null)
        {
            return Response<bool?>.Error(ResponseCode.BadRequest, "L'esdeveniment no existeix.");
        }

        IEnumerable<long> peopleIds = request.PeopleIds.Distinct();
        IEnumerable<Person> people = await _peopleRepository.GetByIdAsync(peopleIds, ct);

        if (peopleIds.Count() != people.Count())
        {
            return Response<bool?>.Error(ResponseCode.BadRequest, "S'han proporcionat identificaadors de persona que no existeixn.");
        }

        IEnumerable<EventPerson> eventPeople = await _eventsPeopleRepository.GetAllByEventIdAsync(e.Id, ct);

        IEnumerable<EventPerson> eventsPeopleToDelte = eventPeople.Where(x => !peopleIds.Contains(x.PersonId));
        if (eventsPeopleToDelte.Any(x => x.Paid))
        {
            return Response<bool?>.Error(ResponseCode.BadRequest, "No es poden eliminar persones que ja han pagat.");
        }


        List<EventPerson> eventsPeopleToAdd = new List<EventPerson>();
        foreach (var p in people)
        {
            var ep = eventPeople.FirstOrDefault(x => x.PersonId == p.Id);
            if (ep == null)
            {
                eventsPeopleToAdd.Add(new EventPerson()
                {
                    Event = e,
                    Person = p,
                    Paid = false,
                    Order = null,
                });
            }
        }

        await _eventsPeopleRepository.DeleteManyAsync(eventsPeopleToDelte, CancellationToken.None);
        await _eventsPeopleRepository.InsertManyAsync(eventsPeopleToAdd, CancellationToken.None);

        return Response<bool?>.Ok(true);
    }


    // move
    private static string RandomString(int length)
    {
        const string pool = "abcdefghijklmnopqrstuvwxyz";
        var builder = new StringBuilder(length);
        var random = new Random();
        for (var i = 0; i < length; i++)
        {
            var c = pool[random.Next(0, pool.Length)];
            builder.Append(c);
        }

        return builder.ToString();
    }
}