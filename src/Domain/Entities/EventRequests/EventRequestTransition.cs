using Domain.Entities.Authentication;

namespace Domain.Entities.EventRequests;

/// <summary>Historial: cada acció sobre una sol·licitud, amb l'estat d'origen i de destí.</summary>
public class EventRequestTransition : Entity
{
    public long EventRequestId { get; set; }
    // Null en la creació de la sol·licitud.
    public EventRequestStatus? From { get; set; }
    public EventRequestStatus To { get; set; }
    public EventRequestAction Action { get; set; }
    // Qui ha fet l'acció o l'ha provocada. Null si no hi ha usuari (sistema).
    public long? ActorUserId { get; set; }
    public User? Actor { get; set; }
    // Transició automàtica del sistema (p. ex. l'esdeveniment s'ha esborrat).
    public bool Automatic { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset Date { get; set; }
}
