using Domain.Entities.Events;

namespace Application.Common.Helpers;

/// <summary>
/// Textos de les autoritzacions que falten. Viuen aquí i no al domini perquè són
/// missatges d'interfície: el domini només decideix si hi ha autorització o no
/// (<see cref="Domain.Entities.People.PersonGroupCourse.IsAuthorizedFor"/>).
/// </summary>
public class AuthorizationMessages
{
    public static string Missing(Event e) => e.Type == EventType.Walking
        ? "Falta l'autorització de sortides a peu"
        : "Falta l'autorització de sortides amb transport";
}
