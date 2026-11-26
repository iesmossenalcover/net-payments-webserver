using Application.Common;
using Domain.Entities.EventRequests;
using Domain.Entities.People;
using Domain.Services;
using FluentValidation;

namespace Application.EventRequests;

#region Data

/// <summary>Dades del formulari de sol·licitud. De moment només els camps mínims.</summary>
public record EventRequestDataDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Price { get; set; }
    public DateTime? PaymentDeadline { get; set; }
    public IEnumerable<EventRequestRecipientDto> Recipients { get; set; } = Enumerable.Empty<EventRequestRecipientDto>();

    public EventRequestData ToDomain() => new(
        Title,
        Description,
        ToUtc(StartDate),
        EndDate.HasValue ? ToUtc(EndDate.Value) : null,
        Price,
        PaymentDeadline.HasValue ? ToUtc(PaymentDeadline.Value) : null,
        Recipients.Select(x => new EventRequestRecipientData(x.Type, x.ReferenceId)).ToList());

    private static DateTimeOffset ToUtc(DateTime d) => new(d.ToUniversalTime(), TimeSpan.Zero);
}

public record EventRequestRecipientDto(EventRequestRecipientType Type, long ReferenceId);

public abstract class EventRequestDataValidator<T> : AbstractValidator<T> where T : EventRequestDataDto
{
    protected EventRequestDataValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("S'ha de proporcionar un títol.")
            .MaximumLength(200).WithMessage("El títol no pot superar els 200 caràcters.");
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("S'ha de seleccionar una data d'inici.");
        RuleFor(x => x.EndDate)
            .Must((r, end) => !end.HasValue || end.Value >= r.StartDate)
            .WithMessage("La data de finalització ha de ser posterior a la data d'inici.");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("S'ha de posar un preu no negatiu.");
        RuleForEach(x => x.Recipients)
            .Must(x => Enum.IsDefined(x.Type)).WithMessage("Tipus de destinatari no vàlid.");
    }
}

public record ReasonBody(string Reason);
public record ApproveBody(DateTime ChangesDeadline, bool SyncToCalendar = false);

#endregion

#region ViewModels

public record EventRequestSummaryVm(
    long Id, string Title, EventRequestStatus Status,
    long RequesterUserId, string RequesterName, string RequesterEmail,
    DateTimeOffset StartDate, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, long? EventId,
    DateTimeOffset? ChangesDeadline);

/// <param name="GroupId">Per als alumnes, el seu grup al curs de la sol·licitud (per agrupar-los al formulari i al detall).</param>
public record EventRequestRecipientVm(EventRequestRecipientType Type, long ReferenceId, string? Name, long? GroupId, string? GroupName);

public record EventRequestTransitionVm(
    EventRequestStatus? From, EventRequestStatus To, EventRequestAction Action,
    long? ActorUserId, string? ActorName, bool Automatic, string? Reason, DateTimeOffset Date);

public record EventRequestVm(
    long Id, string Title, string Description,
    DateTimeOffset StartDate, DateTimeOffset? EndDate, decimal Price, DateTimeOffset? PaymentDeadline,
    EventRequestStatus Status,
    long RequesterUserId, string RequesterName, string RequesterEmail,
    DateTimeOffset? ChangesDeadline, bool CanRequesterEdit, bool SyncToCalendar,
    long? EventId, string? EventCode,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
    IEnumerable<EventRequestRecipientVm> Recipients,
    IEnumerable<EventRequestTransitionVm> Transitions);

#endregion

public static class EventRequestMapper
{
    public static EventRequestSummaryVm ToSummaryVm(EventRequest r) => new(
        r.Id, r.Title, r.Status,
        r.RequesterUserId, $"{r.Requester.Firstname} {r.Requester.Lastname}".Trim(), r.Requester.Username,
        r.StartDate, r.CreatedAt, r.UpdatedAt, r.EventId,
        r.ChangesDeadline);

    /// <summary>Requereix la sol·licitud carregada amb GetWithDetailsAsync. Resol el nom dels destinataris.</summary>
    public static async Task<EventRequestVm> ToVmAsync(EventRequest r, IPeopleRepository peopleRepository, IGroupsRepository groupsRepository, IPersonGroupCourseRepository personGroupCourseRepository, CancellationToken ct)
    {
        var personIds = r.Recipients.Where(x => x.Type == EventRequestRecipientType.Person).Select(x => x.ReferenceId).ToList();
        var groupIds = r.Recipients.Where(x => x.Type == EventRequestRecipientType.Group).Select(x => x.ReferenceId).ToList();

        var peopleNames = personIds.Any()
            ? (await peopleRepository.GetByIdAsync(personIds, ct)).ToDictionary(x => x.Id, x => x.FormalFullName)
            : new Dictionary<long, string>();
        var groupNames = groupIds.Any()
            ? (await groupsRepository.GetByIdAsync(groupIds, ct)).ToDictionary(x => x.Id, x => x.Name)
            : new Dictionary<long, string>();
        var peopleGroups = personIds.Any()
            ? (await personGroupCourseRepository.GetPeopleGroupByPeopleIdsAndCourseIdAsync(r.CourseId, personIds, ct))
                .GroupBy(x => x.PersonId).ToDictionary(x => x.Key, x => x.First().Group)
            : new Dictionary<long, Group>();

        return new EventRequestVm(
            r.Id, r.Title, r.Description,
            r.StartDate, r.EndDate, r.Price, r.PaymentDeadline,
            r.Status,
            r.RequesterUserId, $"{r.Requester.Firstname} {r.Requester.Lastname}".Trim(), r.Requester.Username,
            r.ChangesDeadline,
            r.Status == EventRequestStatus.PendingCorrection || r.CanRequesterEditApproved(DateTimeOffset.UtcNow),
            r.SyncToCalendar,
            r.EventId, r.Event?.Code,
            r.CreatedAt, r.UpdatedAt,
            r.Recipients.Select(x =>
            {
                if (x.Type == EventRequestRecipientType.Group)
                {
                    return new EventRequestRecipientVm(x.Type, x.ReferenceId, groupNames.GetValueOrDefault(x.ReferenceId), null, null);
                }
                var group = peopleGroups.GetValueOrDefault(x.ReferenceId);
                return new EventRequestRecipientVm(x.Type, x.ReferenceId, peopleNames.GetValueOrDefault(x.ReferenceId), group?.Id, group?.Name);
            }),
            r.Transitions.OrderBy(x => x.Date).Select(x => new EventRequestTransitionVm(
                x.From, x.To, x.Action,
                x.ActorUserId, x.Actor != null ? $"{x.Actor.Firstname} {x.Actor.Lastname}".Trim() : null,
                x.Automatic, x.Reason, x.Date)));
    }
}

/// <summary>
/// Pas comú de totes les accions sobre una sol·licitud: carregar-la, aplicar l'acció de domini i desar
/// condicionat a l'estat llegit. Tradueix els errors de negoci a respostes.
/// </summary>
public static class EventRequestActions
{
    public const string STATUS_CHANGED_MESSAGE = "Aquesta sol·licitud ja ha canviat d'estat.";
    public const string NOT_FOUND_MESSAGE = "La sol·licitud no existeix.";
    public const string NO_USER_MESSAGE = "No s'ha pogut identificar l'usuari.";

    /// <param name="ownerUserId">Si s'indica, només el propietari la pot veure (els altres reben NotFound).</param>
    /// <param name="afterSave">S'executa només si s'ha desat (p. ex. enviar correus).</param>
    public static async Task<Response<long?>> RunAsync(
        IEventRequestsRepository repository,
        long id,
        long? ownerUserId,
        Action<EventRequest, DateTimeOffset> apply,
        Func<EventRequest, Task>? afterSave,
        CancellationToken ct)
    {
        EventRequest? request = await repository.GetWithDetailsAsync(id, ct);
        if (request == null || (ownerUserId.HasValue && !request.IsOwnedBy(ownerUserId.Value)))
        {
            return Response<long?>.Error(ResponseCode.NotFound, NOT_FOUND_MESSAGE);
        }

        try
        {
            apply(request, DateTimeOffset.UtcNow);
        }
        catch (EventRequestException e)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, e.Message);
        }

        if (!await repository.SaveAsync(CancellationToken.None))
        {
            return Response<long?>.Error(ResponseCode.BadRequest, STATUS_CHANGED_MESSAGE);
        }

        if (afterSave != null) await afterSave(request);

        return Response<long?>.Ok(request.Id);
    }
}
