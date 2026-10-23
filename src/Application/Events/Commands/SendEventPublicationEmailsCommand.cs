using System.Globalization;
using System.Net;
using System.Text;
using Application.Common;
using Domain.Entities.Events;
using Domain.Services;
using MediatR;

namespace Application.Events.Commands;

/// <summary>
/// Envia un correu per event quan s'obre (PublishDate) i quan es tanca (UnpublishDate) el pagament.
/// Cada avís es marca com a processat encara que l'event no tingui NotifyExtracurricular,
/// perquè activar-lo més tard no faci arribar avisos fora de temps.
/// </summary>
public record SendEventPublicationEmailsCommand : IRequest<Response<SendEventPublicationEmailsCommandVm>>;

public record SendEventPublicationEmailsCommandVm(int Sent, int Failed);

public class SendEventPublicationEmailsCommandHandler : IRequestHandler<SendEventPublicationEmailsCommand, Response<SendEventPublicationEmailsCommandVm>>
{
    #region props
    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly IEventsRespository _eventsRepository;
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly ILogger<SendEventPublicationEmailsCommandHandler> _logger;
    private readonly string recipient;
    private readonly string frontEventPaymentsUrl;
    private readonly TimeZoneInfo timeZone;

    public SendEventPublicationEmailsCommandHandler(
        IGoogleAdminApi googleAdminApi,
        IEventsRespository eventsRepository,
        IEventsPeopleRespository eventsPeopleRepository,
        IConfiguration configuration,
        ILogger<SendEventPublicationEmailsCommandHandler> logger)
    {
        _googleAdminApi = googleAdminApi;
        _eventsRepository = eventsRepository;
        _eventsPeopleRepository = eventsPeopleRepository;
        _logger = logger;
        // Admet diversos destinataris separats per comes: van tots a la capçalera To.
        string[] recipients = (configuration.GetValue<string>("EventPublicationEmailRecipient") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (recipients.Length == 0) throw new Exception("EventPublicationEmailRecipient");
        recipient = string.Join(", ", recipients);
        frontEventPaymentsUrl = configuration.GetValue<string>("FrontEventPaymentsUrl") ?? throw new Exception("FrontEventPaymentsUrl");
        timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration.GetValue<string>("DailyEventsEmailTimeZone") ?? "Europe/Madrid");
    }
    #endregion

    public async Task<Response<SendEventPublicationEmailsCommandVm>> Handle(SendEventPublicationEmailsCommand request, CancellationToken ct)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        IEnumerable<Event> events = await _eventsRepository.GetEventsPendingPublicationNoticeAsync(now, ct);

        int sent = 0;
        int failed = 0;

        foreach (Event e in events)
        {
            bool changed = false;

            if (!e.PublishNotifiedAt.HasValue && e.PublishDate <= now)
            {
                if (await NotifyAsync(e, opened: true, ct)) { e.PublishNotifiedAt = now; changed = true; sent += e.NotifyExtracurricular ? 1 : 0; }
                else failed++;
            }

            if (!e.UnpublishNotifiedAt.HasValue && e.UnpublishDate.HasValue && e.UnpublishDate.Value <= now)
            {
                if (await NotifyAsync(e, opened: false, ct)) { e.UnpublishNotifiedAt = now; changed = true; sent += e.NotifyExtracurricular ? 1 : 0; }
                else failed++;
            }

            if (changed) await _eventsRepository.UpdateAsync(e, CancellationToken.None);
        }

        if (failed > 0)
        {
            return Response<SendEventPublicationEmailsCommandVm>.Error(ResponseCode.InternalError, $"No s'han pogut enviar {failed} avisos de publicació d'events");
        }

        return Response<SendEventPublicationEmailsCommandVm>.Ok(new SendEventPublicationEmailsCommandVm(sent, failed));
    }

    // Retorna si l'avís s'ha de donar per processat (enviat o no calia enviar-lo).
    private async Task<bool> NotifyAsync(Event e, bool opened, CancellationToken ct)
    {
        if (!e.NotifyExtracurricular) return true;

        IEnumerable<EventPerson> people = await _eventsPeopleRepository.GetAllByEventIdAsync(e.Id, ct);
        int paid = people.Count(x => x.Paid);
        int unpaid = people.Count(x => !x.Paid);

        string action = opened ? "S'obre el pagament" : "Es tanca el pagament";
        string subject = $"[EXTRAESCOLARS] {action}: {e.Name} ({e.Code})";
        string body = BuildBody(e, action, paid, unpaid);

        var result = await _googleAdminApi.SendHtmlEmail(recipient, subject, body, ct);
        if (!result.Success)
        {
            _logger.LogError("Error enviant l'avís '{Action}' de l'event {Code}: {Error}", action, e.Code, result.ErrorMessage);
            return false;
        }

        _logger.LogInformation("Enviat l'avís '{Action}' de l'event {Code}", action, e.Code);
        return true;
    }

    private string BuildBody(Event e, string action, int paid, int unpaid)
    {
        string url = frontEventPaymentsUrl.Replace("{code}", Uri.EscapeDataString(e.Code));
        string capacity = e.MaxCapacity.HasValue ? e.MaxCapacity.Value.ToString(CultureInfo.InvariantCulture) : "Sense límit";

        StringBuilder html = new StringBuilder();
        html.Append("<html><body style=\"font-family: Arial, Helvetica, sans-serif; color: #222;\">");
        html.Append($"<p>{Escape(action)} de l'esdeveniment:</p>");
        html.Append("<div style=\"margin: 0 0 24px 0; padding: 12px 16px; border-left: 4px solid #d81b60;\">");
        html.Append($"<h3 style=\"margin: 0 0 8px 0;\">{Escape(e.Name)}</h3>");
        html.Append("<table style=\"border-collapse: collapse;\">");
        Row(html, "Codi", e.Code);
        Row(html, "Places màximes", capacity);
        Row(html, "Han pagat", paid.ToString(CultureInfo.InvariantCulture));
        Row(html, "No han pagat", unpaid.ToString(CultureInfo.InvariantCulture));
        html.Append("</table>");
        html.Append($"<p style=\"margin: 8px 0 0 0;\"><a href=\"{Escape(url)}\">Veure els pagaments</a></p>");
        html.Append("</div>");
        html.Append("<p style=\"color: #888; font-size: 12px;\">Correu automàtic generat per l'aplicació de pagaments.</p>");
        html.Append("</body></html>");

        return html.ToString();
    }

    private static void Row(StringBuilder html, string label, string value)
    {
        html.Append($"<tr><td style=\"padding: 2px 16px 2px 0; color: #555;\">{Escape(label)}</td><td style=\"padding: 2px 0;\"><strong>{Escape(value)}</strong></td></tr>");
    }

    private static string Escape(string value) => WebUtility.HtmlEncode(value);
}
