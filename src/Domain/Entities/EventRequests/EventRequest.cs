using Domain.Entities.Authentication;
using Domain.Entities.Events;
using Domain.Entities.People;

namespace Domain.Entities.EventRequests;

public enum EventRequestStatus
{
    PendingReview = 0,
    PendingCorrection = 1,
    PendingCreation = 2,
    Created = 3,
    Deleted = 4,
}

public enum EventRequestAction
{
    Submit = 0,
    Approve = 1,
    RequestCorrection = 2,
    Edit = 3,
    Resubmit = 4,
    CreateEvent = 5,
    EventDeleted = 6,
    Reject = 7,
    Delete = 8,
}

/// <summary>
/// Dades que proposa el professor. De moment només els camps mínims: el formulari s'ampliarà més endavant.
/// </summary>
public record EventRequestData(
    string Title,
    string Description,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    decimal Price,
    DateTimeOffset? PaymentDeadline,
    IReadOnlyCollection<EventRequestRecipientData> Recipients);

public record EventRequestRecipientData(EventRequestRecipientType Type, long ReferenceId);

/// <summary>
/// Sol·licitud d'un professor per crear un esdeveniment. Tots els canvis d'estat passen pels mètodes
/// d'aquesta classe, que validen la transició i en deixen constància a <see cref="Transitions"/>.
/// La concurrència es controla a la bbdd: <see cref="Status"/> és token de concurrència, així que
/// l'UPDATE només s'aplica si l'estat continua sent el que s'ha llegit.
/// </summary>
public class EventRequest : Entity
{
    public long RequesterUserId { get; private set; }
    public User Requester { get; private set; } = default!;

    public long CourseId { get; private set; }
    public Course Course { get; private set; } = default!;

    public EventRequestStatus Status { get; private set; }

    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    public decimal Price { get; private set; }
    public DateTimeOffset? PaymentDeadline { get; private set; }

    public long? EventId { get; private set; }
    public Event? Event { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public List<EventRequestRecipient> Recipients { get; private set; } = new();
    public List<EventRequestTransition> Transitions { get; private set; } = new();

    private EventRequest() { }

    public static EventRequest Submit(long requesterUserId, long courseId, EventRequestData data, DateTimeOffset now)
    {
        var request = new EventRequest
        {
            RequesterUserId = requesterUserId,
            CourseId = courseId,
            Status = EventRequestStatus.PendingReview,
            CreatedAt = now,
        };
        request.SetData(data, now);
        request.AddTransition(null, EventRequestStatus.PendingReview, EventRequestAction.Submit, requesterUserId, false, null, now);
        return request;
    }

    public bool IsOwnedBy(long userId) => RequesterUserId == userId;

    public void Approve(long adminUserId, DateTimeOffset now)
    {
        EnsureStatus(EventRequestStatus.PendingReview);
        ChangeStatus(EventRequestStatus.PendingCreation, EventRequestAction.Approve, adminUserId, false, null, now);
    }

    public void RequestCorrection(long adminUserId, string reason, DateTimeOffset now)
    {
        EnsureStatus(EventRequestStatus.PendingReview, EventRequestStatus.PendingCreation);
        ChangeStatus(EventRequestStatus.PendingCorrection, EventRequestAction.RequestCorrection, adminUserId, false, RequireReason(reason), now);
    }

    /// <summary>
    /// L'admin edita en revisió (no canvia l'estat) o en pendent de creació (torna a revisió automàticament).
    /// No pot editar en correcció perquè és el torn del professor.
    /// </summary>
    public void EditByAdmin(long adminUserId, EventRequestData data, DateTimeOffset now)
    {
        EnsureStatus(EventRequestStatus.PendingReview, EventRequestStatus.PendingCreation);
        SetData(data, now);
        // Queda constància de l'edició encara que l'estat no canviï.
        ChangeStatus(EventRequestStatus.PendingReview, EventRequestAction.Edit, adminUserId, Status == EventRequestStatus.PendingCreation, null, now);
    }

    /// <summary>El professor propietari corregeix i torna a enviar. Només pot editar en correcció.</summary>
    public void Resubmit(long userId, EventRequestData data, DateTimeOffset now)
    {
        if (!IsOwnedBy(userId)) throw new EventRequestException("Aquesta sol·licitud no és teva.");
        EnsureStatus(EventRequestStatus.PendingCorrection);
        SetData(data, now);
        ChangeStatus(EventRequestStatus.PendingReview, EventRequestAction.Resubmit, userId, false, null, now);
    }

    public void MarkCreated(long adminUserId, long eventId, DateTimeOffset now)
    {
        EnsureStatus(EventRequestStatus.PendingCreation);
        EventId = eventId;
        ChangeStatus(EventRequestStatus.Created, EventRequestAction.CreateEvent, adminUserId, false, null, now);
    }

    /// <summary>
    /// S'ha esborrat l'esdeveniment creat: la sol·licitud torna a revisió. És una transició automàtica
    /// (del sistema), però es guarda qui ha esborrat l'esdeveniment si se sap.
    /// </summary>
    public void OnEventDeleted(long? actorUserId, DateTimeOffset now)
    {
        EnsureStatus(EventRequestStatus.Created);
        EventId = null;
        ChangeStatus(EventRequestStatus.PendingReview, EventRequestAction.EventDeleted, actorUserId, true, null, now);
    }

    public void Reject(long adminUserId, string reason, DateTimeOffset now)
    {
        EnsureStatus(EventRequestStatus.PendingReview, EventRequestStatus.PendingCorrection, EventRequestStatus.PendingCreation);
        DeletedAt = now;
        ChangeStatus(EventRequestStatus.Deleted, EventRequestAction.Reject, adminUserId, false, RequireReason(reason), now);
    }

    public void Delete(long adminUserId, DateTimeOffset now)
    {
        EnsureStatus(EventRequestStatus.PendingReview, EventRequestStatus.PendingCorrection, EventRequestStatus.PendingCreation);
        DeletedAt = now;
        ChangeStatus(EventRequestStatus.Deleted, EventRequestAction.Delete, adminUserId, false, null, now);
    }

    #region helpers

    private void SetData(EventRequestData data, DateTimeOffset now)
    {
        Title = data.Title.Trim();
        Description = data.Description.Trim();
        StartDate = data.StartDate;
        EndDate = data.EndDate;
        Price = data.Price;
        PaymentDeadline = data.PaymentDeadline;

        Recipients.Clear();
        foreach (var r in data.Recipients.Distinct())
        {
            Recipients.Add(new EventRequestRecipient { Type = r.Type, ReferenceId = r.ReferenceId });
        }

        UpdatedAt = now;
    }

    private void EnsureStatus(params EventRequestStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new EventRequestException("Aquesta acció no es pot fer en l'estat actual de la sol·licitud.");
        }
    }

    private static string RequireReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new EventRequestException("S'ha d'indicar el motiu.");
        return reason.Trim();
    }

    private void ChangeStatus(EventRequestStatus to, EventRequestAction action, long? actorUserId, bool automatic, string? reason, DateTimeOffset now)
    {
        EventRequestStatus from = Status;
        Status = to;
        UpdatedAt = now;
        AddTransition(from, to, action, actorUserId, automatic, reason, now);
    }

    private void AddTransition(EventRequestStatus? from, EventRequestStatus to, EventRequestAction action, long? actorUserId, bool automatic, string? reason, DateTimeOffset now)
    {
        Transitions.Add(new EventRequestTransition
        {
            From = from,
            To = to,
            Action = action,
            ActorUserId = actorUserId,
            Automatic = automatic,
            Reason = reason,
            Date = now,
        });
    }

    #endregion
}

/// <summary>Error de regla de negocio en una sol·licitud: transició no permesa, falta el motiu, etc.</summary>
public class EventRequestException : Exception
{
    public EventRequestException(string message) : base(message) { }
}
