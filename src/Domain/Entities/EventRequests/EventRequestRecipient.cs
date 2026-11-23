namespace Domain.Entities.EventRequests;

public enum EventRequestRecipientType
{
    Person = 0,
    Group = 1,
}

/// <summary>Alumne o grup que el professor proposa com a participant de l'esdeveniment.</summary>
public class EventRequestRecipient : Entity
{
    public long EventRequestId { get; set; }
    public EventRequestRecipientType Type { get; set; }
    public long ReferenceId { get; set; }
}
