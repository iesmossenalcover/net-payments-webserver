using Domain.Behaviours;

namespace Application.Orders;

public static class PayOrderResultExtensions
{
    /// <summary>
    /// Missatge d'error per mostrar, o null si l'ordre ha quedat liquidada.
    /// Una ordre que ja estava pagada no és un error: el resultat per a qui ho demana és el mateix.
    /// </summary>
    public static string? ErrorMessage(this PayOrderResult result) => result switch
    {
        PayOrderResult.Ok => null,
        PayOrderResult.AlreadyPaid => null,
        PayOrderResult.NoEventsForOrder => "Error, cap esdeveniment amb aquest ordre",
        PayOrderResult.PersonNotInCourse => "Error, la persona no està asociada al curs",
        _ => "Error en confirmar l'ordre.",
    };
}
