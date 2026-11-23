using System.Globalization;
using System.Net;
using System.Text;
using Domain.Entities.EventRequests;
using Domain.Services;

namespace Application.EventRequests;

/// <summary>
/// Correus automàtics de les sol·licituds. S'envien després de desar: si falla l'enviament,
/// l'acció ja està feta i només es registra l'error.
/// Configuració:
///  - EventRequestAdminEmailRecipient: destinataris de l'admin, separats per comes.
///  - FrontEventRequestUrl: enllaç del professor a la seva sol·licitud ({id}).
///  - FrontAdminEventRequestUrl: enllaç de l'admin a la sol·licitud ({id}).
/// Si falta alguna clau, no s'envia el correu corresponent i es deixa un avís al log.
/// </summary>
public class EventRequestNotifier
{
    #region IOC
    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly ILogger<EventRequestNotifier> _logger;
    private readonly string? adminRecipient;
    private readonly string? frontRequestUrl;
    private readonly string? frontAdminRequestUrl;
    private readonly TimeZoneInfo timeZone;

    public EventRequestNotifier(IGoogleAdminApi googleAdminApi, IConfiguration configuration, ILogger<EventRequestNotifier> logger)
    {
        _googleAdminApi = googleAdminApi;
        _logger = logger;
        string[] recipients = (configuration.GetValue<string>("EventRequestAdminEmailRecipient") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        adminRecipient = recipients.Length > 0 ? string.Join(", ", recipients) : null;
        frontRequestUrl = configuration.GetValue<string>("FrontEventRequestUrl");
        frontAdminRequestUrl = configuration.GetValue<string>("FrontAdminEventRequestUrl");
        // Les dates del correu es mostren en hora local de l'escola.
        timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration.GetValue<string>("DailyEventsEmailTimeZone") ?? "Europe/Madrid");
    }
    #endregion

    #region Admin

    public Task NotifySubmittedAsync(EventRequest r, CancellationToken ct) =>
        SendToAdminAsync(r, "Nova sol·licitud d'esdeveniment", "Hi ha una sol·licitud nova pendent de revisar.", ct);

    public Task NotifyResubmittedAsync(EventRequest r, CancellationToken ct) =>
        SendToAdminAsync(r, "Sol·licitud corregida", "El professor ha corregit la sol·licitud i torna a estar pendent de revisar.", ct);

    public Task NotifyEditedAfterApprovalAsync(EventRequest r, CancellationToken ct) =>
        SendToAdminAsync(r, "Sol·licitud aprovada modificada", "El professor ha fet canvis a una sol·licitud aprovada i torna a estar pendent de revisar.", ct);

    #endregion

    #region Professor

    public Task NotifyApprovedAsync(EventRequest r, CancellationToken ct)
    {
        string deadline = r.ChangesDeadline.HasValue
            ? TimeZoneInfo.ConvertTime(r.ChangesDeadline.Value, timeZone).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
            : "-";
        return SendToRequesterAsync(r, "Sol·licitud aprovada",
            $"La sol·licitud s'ha aprovat i l'administració crearà l'esdeveniment. Tens fins al {deadline} per fer-hi canvis; si en fas, tornarà a estar pendent de revisió.",
            null, true, ct);
    }

    public Task NotifyCorrectionRequestedAsync(EventRequest r, string reason, CancellationToken ct) =>
        SendToRequesterAsync(r, "Cal corregir la sol·licitud", "L'administració demana que corregeixis la sol·licitud.", reason, true, ct);

    public Task NotifyRejectedAsync(EventRequest r, string reason, CancellationToken ct) =>
        SendToRequesterAsync(r, "Sol·licitud rebutjada", "La sol·licitud s'ha rebutjat.", reason, false, ct);

    public Task NotifyDeletedAsync(EventRequest r, CancellationToken ct) =>
        SendToRequesterAsync(r, "Sol·licitud eliminada", "La sol·licitud s'ha eliminat.", null, false, ct);

    public Task NotifyEventCreatedAsync(EventRequest r, string eventCode, CancellationToken ct) =>
        SendToRequesterAsync(r, "Esdeveniment creat", $"S'ha creat l'esdeveniment de la sol·licitud (codi {eventCode}).", null, true, ct);

    #endregion

    #region helpers

    private async Task SendToAdminAsync(EventRequest r, string title, string message, CancellationToken ct)
    {
        if (adminRecipient == null)
        {
            _logger.LogWarning("No s'envia el correu '{Title}' de la sol·licitud {Id}: falta EventRequestAdminEmailRecipient", title, r.Id);
            return;
        }

        string? url = frontAdminRequestUrl?.Replace("{id}", r.Id.ToString(CultureInfo.InvariantCulture));
        await SendAsync(adminRecipient, title, BuildBody(r, message, null, url), r.Id, ct);
    }

    private async Task SendToRequesterAsync(EventRequest r, string title, string message, string? reason, bool withLink, CancellationToken ct)
    {
        // El Username dels usuaris de Google és el correu.
        string to = r.Requester.Username;
        // Les sol·licituds eliminades ja no les veu ningú: no té sentit enllaçar-hi.
        string? url = withLink ? frontRequestUrl?.Replace("{id}", r.Id.ToString(CultureInfo.InvariantCulture)) : null;
        await SendAsync(to, title, BuildBody(r, message, reason, url), r.Id, ct);
    }

    private async Task SendAsync(string to, string title, string body, long requestId, CancellationToken ct)
    {
        string subject = $"[SOL·LICITUDS] {title}";
        try
        {
            var result = await _googleAdminApi.SendHtmlEmail(to, subject, body, ct);
            if (!result.Success)
            {
                _logger.LogError("Error enviant el correu '{Title}' de la sol·licitud {Id}: {Error}", title, requestId, result.ErrorMessage);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error enviant el correu '{Title}' de la sol·licitud {Id}", title, requestId);
        }
    }

    private static string BuildBody(EventRequest r, string message, string? reason, string? url)
    {
        StringBuilder html = new StringBuilder();
        html.Append("<html><body style=\"font-family: Arial, Helvetica, sans-serif; color: #222;\">");
        html.Append($"<p>{Escape(message)}</p>");
        html.Append("<div style=\"margin: 0 0 24px 0; padding: 12px 16px; border-left: 4px solid #7e57c2;\">");
        html.Append($"<h3 style=\"margin: 0 0 8px 0;\">{Escape(r.Title)}</h3>");
        html.Append($"<p style=\"margin: 0;\">Sol·licitada per {Escape($"{r.Requester.Firstname} {r.Requester.Lastname}".Trim())}</p>");
        if (!string.IsNullOrWhiteSpace(reason))
        {
            html.Append($"<p style=\"margin: 8px 0 0 0;\"><strong>Motiu:</strong> {Escape(reason)}</p>");
        }
        if (url != null)
        {
            html.Append($"<p style=\"margin: 8px 0 0 0;\"><a href=\"{Escape(url)}\">Veure la sol·licitud</a></p>");
        }
        html.Append("</div>");
        html.Append("<p style=\"color: #888; font-size: 12px;\">Correu automàtic generat per l'aplicació de pagaments.</p>");
        html.Append("</body></html>");
        return html.ToString();
    }

    private static string Escape(string value) => WebUtility.HtmlEncode(value);

    #endregion
}
