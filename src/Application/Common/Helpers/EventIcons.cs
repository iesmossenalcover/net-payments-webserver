using Domain.Entities.Events;

namespace Application.Common.Helpers;

/// <summary>
/// Icona (emoji) que es mostra davant el títol de l'esdeveniment al calendari i al correu diari.
/// </summary>
public class EventIcons
{
    public static string For(EventType type) => type switch
    {
        EventType.Trip => "🧳",
        EventType.Transport => "🚌",
        EventType.Walking => "👣",
        _ => "🏫",
    };
}
