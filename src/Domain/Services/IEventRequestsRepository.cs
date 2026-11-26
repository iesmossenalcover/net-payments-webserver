using Domain.Entities.EventRequests;

namespace Domain.Services;

public interface IEventRequestsRepository : IRepository<EventRequest>
{
    /// <summary>Amb sol·licitant, destinataris i historial. No retorna les eliminades.</summary>
    Task<EventRequest?> GetWithDetailsAsync(long id, CancellationToken ct);
    /// <summary>Sol·licituds d'un usuari, sense les eliminades.</summary>
    /// <summary>Sol·licituds d'un professor en un curs, sense les eliminades.</summary>
    Task<IEnumerable<EventRequest>> GetAllByRequesterAsync(long userId, long courseId, CancellationToken ct);
    /// <summary>Totes les sol·licituds d'un curs (opcionalment d'un estat), sense les eliminades.</summary>
    Task<IEnumerable<EventRequest>> GetAllAsync(EventRequestStatus? status, long courseId, CancellationToken ct);
    Task<EventRequest?> GetByEventIdAsync(long eventId, CancellationToken ct);
    /// <summary>Sol·licituds no eliminades ni ja creades (també les rebutjades) que ocupen algun moment de [from, to].</summary>
    Task<IEnumerable<EventRequest>> GetCalendarOverlappingAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    /// <summary>
    /// Desa els canvis fets a les sol·licituds carregades. Retorna false si l'estat d'alguna ha canviat
    /// mentrestant (una altra acció ha arribat abans).
    /// </summary>
    Task<bool> SaveAsync(CancellationToken ct);
}
