using Application.Common;
using Application.Events.Commands;
using Domain.Entities.EventRequests;
using Domain.Entities.Events;
using Domain.Services;
using MediatR;

namespace Application.EventRequests.Commands;

/// <summary>
/// L'admin crea l'esdeveniment a partir d'una sol·licitud pendent de creació. Les dades de l'esdeveniment
/// les envia l'admin (el front les precarrega des de la sol·licitud) i es validen com en crear-ne un.
/// Tot va en una transacció: crear l'esdeveniment, assignar-hi els destinataris proposats i passar la
/// sol·licitud a Created. Si qualsevol pas falla, no queda res fet i la sol·licitud continua pendent de creació.
/// Un doble clic no en crea dos: el canvi d'estat és condicional i l'EventId de la sol·licitud és únic.
/// </summary>
public record CreateEventFromRequestCommand(long Id, CreateEventCommand Event) : IRequest<Response<string?>>;

public class CreateEventFromRequestCommandHandler : IRequestHandler<CreateEventFromRequestCommand, Response<string?>>
{
    #region IOC
    private readonly IEventRequestsRepository _eventRequestsRepository;
    private readonly IEventsRespository _eventsRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly IPeopleRepository _peopleRepository;
    private readonly ICurrentRequestService _currentRequestService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;
    private readonly EventRequestNotifier _notifier;

    public CreateEventFromRequestCommandHandler(
        IEventRequestsRepository eventRequestsRepository,
        IEventsRespository eventsRepository,
        IEventsPeopleRespository eventsPeopleRepository,
        IPersonGroupCourseRepository personGroupCourseRepository,
        IPeopleRepository peopleRepository,
        ICurrentRequestService currentRequestService,
        IUnitOfWork unitOfWork,
        ISender sender,
        EventRequestNotifier notifier)
    {
        _eventRequestsRepository = eventRequestsRepository;
        _eventsRepository = eventsRepository;
        _eventsPeopleRepository = eventsPeopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _peopleRepository = peopleRepository;
        _currentRequestService = currentRequestService;
        _unitOfWork = unitOfWork;
        _sender = sender;
        _notifier = notifier;
    }
    #endregion

    public async Task<Response<string?>> Handle(CreateEventFromRequestCommand request, CancellationToken ct)
    {
        if (_currentRequestService.UserId is not long userId)
        {
            return Response<string?>.Error(ResponseCode.BadRequest, EventRequestActions.NO_USER_MESSAGE);
        }

        EventRequest? eventRequest = await _eventRequestsRepository.GetWithDetailsAsync(request.Id, ct);
        if (eventRequest == null)
        {
            return Response<string?>.Error(ResponseCode.NotFound, EventRequestActions.NOT_FOUND_MESSAGE);
        }
        if (eventRequest.Status != EventRequestStatus.PendingCreation)
        {
            return Response<string?>.Error(ResponseCode.BadRequest, "Només es pot crear l'esdeveniment d'una sol·licitud aprovada.");
        }

        Response<string?>? result = null;
        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async _ =>
            {
                // Passa per MediatR perquè s'apliquin les mateixes validacions que en crear un esdeveniment.
                Response<string?> created = await _sender.Send(request.Event, CancellationToken.None);
                if (created.Code != ResponseCode.Success || created.Data == null)
                {
                    result = created;
                    throw new AbortException();
                }

                Event e = await _eventsRepository.GetEventByCodeAsync(created.Data, CancellationToken.None)
                    ?? throw new Exception("No s'ha trobat l'esdeveniment acabat de crear");

                await AddRecipientsAsync(eventRequest, e, CancellationToken.None);

                eventRequest.MarkCreated(userId, e.Id, DateTimeOffset.UtcNow);
                if (!await _eventRequestsRepository.SaveAsync(CancellationToken.None))
                {
                    result = Response<string?>.Error(ResponseCode.BadRequest, EventRequestActions.STATUS_CHANGED_MESSAGE);
                    throw new AbortException();
                }

                result = Response<string?>.Ok(e.Code);
            }, ct);
        }
        catch (AbortException)
        {
            // Rollback fet en sortir de la transacció. result ja té l'error.
        }
        catch (EventRequestException ex)
        {
            result = Response<string?>.Error(ResponseCode.BadRequest, ex.Message);
        }

        if (result!.Code == ResponseCode.Success && result.Data != null)
        {
            await _notifier.NotifyEventCreatedAsync(eventRequest, result.Data, CancellationToken.None);
        }

        return result;
    }

    /// <summary>Assigna a l'esdeveniment els alumnes proposats: els indicats un a un i els dels grups (curs de l'esdeveniment).</summary>
    private async Task AddRecipientsAsync(EventRequest eventRequest, Event e, CancellationToken ct)
    {
        var personIds = eventRequest.Recipients
            .Where(x => x.Type == EventRequestRecipientType.Person)
            .Select(x => x.ReferenceId)
            .ToHashSet();

        foreach (long groupId in eventRequest.Recipients.Where(x => x.Type == EventRequestRecipientType.Group).Select(x => x.ReferenceId))
        {
            var members = await _personGroupCourseRepository.GetPeopleGroupByGroupIdAndCourseIdAsync(e.CourseId, groupId, ct);
            personIds.UnionWith(members.Select(x => x.PersonId));
        }

        if (personIds.Count == 0) return;

        // Si algun alumne proposat ja no existeix, simplement no s'afegeix.
        var people = await _peopleRepository.GetByIdAsync(personIds, ct);
        var eventPeople = people.Select(p => new EventPerson
        {
            EventId = e.Id,
            PersonId = p.Id,
            Quantity = 1,
            Paid = false,
        }).ToList();

        await _eventsPeopleRepository.InsertManyAsync(eventPeople, ct);
    }

    private class AbortException : Exception { }
}
