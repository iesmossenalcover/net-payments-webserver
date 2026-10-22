using System.Globalization;
using System.Net;
using System.Text;
using Application.Common;
using Application.Common.Helpers;
using Domain.Entities.Events;
using Domain.Entities.People;
using Domain.Services;
using MediatR;

namespace Application.Events.Commands;

/// <summary>
/// Prepara i envia el resum dels events que comencen un dia concret.
/// Si no s'indica <paramref name="Day"/> s'agafa el dia d'avui a la zona horària configurada.
/// </summary>
public record SendDailyEventsEmailCommand(DateOnly? Day = null) : IRequest<Response<SendDailyEventsEmailCommandVm>>;

public record SendDailyEventsEmailCommandVm(bool Sent, int EventsCount, string? MessageId);

public class SendDailyEventsEmailCommandHandler : IRequestHandler<SendDailyEventsEmailCommand, Response<SendDailyEventsEmailCommandVm>>
{
    #region props
    private readonly IGoogleAdminApi _googleAdminApi;
    private readonly IEventsRespository _eventsRepository;
    private readonly ILogger<SendDailyEventsEmailCommandHandler> _logger;
    private readonly string recipient;
    private readonly string frontEventSummaryUrl;
    private readonly TimeZoneInfo timeZone;

    public SendDailyEventsEmailCommandHandler(
        IGoogleAdminApi googleAdminApi,
        IEventsRespository eventsRepository,
        IConfiguration configuration,
        ILogger<SendDailyEventsEmailCommandHandler> logger)
    {
        _googleAdminApi = googleAdminApi;
        _eventsRepository = eventsRepository;
        _logger = logger;
        recipient = configuration.GetValue<string>("DailyEventsEmailRecipient") ?? throw new Exception("DailyEventsEmailRecipient");
        frontEventSummaryUrl = configuration.GetValue<string>("FrontEventSummaryUrl") ?? throw new Exception("FrontEventSummaryUrl");
        timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration.GetValue<string>("DailyEventsEmailTimeZone") ?? "Europe/Madrid");
    }
    #endregion

    public async Task<Response<SendDailyEventsEmailCommandVm>> Handle(SendDailyEventsEmailCommand request, CancellationToken ct)
    {
        DateOnly day = request.Day ?? DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone).DateTime);

        DateTimeOffset from = DayStart(day);
        DateTimeOffset to = DayStart(day.AddDays(1));

        IEnumerable<Event> events = await _eventsRepository.GetEventsStartingBetweenAsync(from, to, ct);

        // Les matrícules i els events només per a l'AMIPA no són excursions: no interessen al claustre.
        List<Event> excursions = events.Where(x => !x.Enrollment && !x.Amipa).ToList();

        if (excursions.Count == 0)
        {
            _logger.LogInformation("Resum diari d'events del {Day}: cap event, no s'envia cap correu", day);
            return Response<SendDailyEventsEmailCommandVm>.Ok(new SendDailyEventsEmailCommandVm(false, 0, null));
        }

        string subject = $"[EXTRAESCOLARS] Activitats d'avui {FormatDate(day)}";
        string body = BuildBody(day, excursions);

        var result = await _googleAdminApi.SendHtmlEmail(recipient, subject, body, ct);
        if (!result.Success)
        {
            return Response<SendDailyEventsEmailCommandVm>.Error(ResponseCode.InternalError, result.ErrorMessage ?? "Error enviant el resum diari d'events");
        }

        return Response<SendDailyEventsEmailCommandVm>.Ok(new SendDailyEventsEmailCommandVm(true, excursions.Count, result.Data));
    }

    private DateTimeOffset DayStart(DateOnly day)
    {
        DateTime local = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, timeZone.GetUtcOffset(local));
    }

    private string BuildBody(DateOnly day, IEnumerable<Event> events)
    {
        StringBuilder html = new StringBuilder();
        html.Append("<html><body style=\"font-family: Arial, Helvetica, sans-serif; color: #222;\">");
        html.Append($"<p>Activitats previstes per a avui, {Escape(FormatDate(day))}:</p>");

        foreach (Event e in events)
        {
            string url = frontEventSummaryUrl.Replace("{code}", Uri.EscapeDataString(e.Code));
            string when = string.IsNullOrWhiteSpace(e.Location) ? TimeRange(e) : $"{TimeRange(e)} ({e.Location.Trim()})";

            html.Append("<div style=\"margin: 0 0 24px 0; padding: 12px 16px; border-left: 4px solid #d81b60;\">");
            html.Append($"<h3 style=\"margin: 0 0 8px 0;\">{Escape(e.Name)}</h3>");
            html.Append($"<p style=\"margin: 0 0 8px 0; color: #555;\">{Escape(when)}</p>");

            if (!string.IsNullOrWhiteSpace(e.Description))
            {
                html.Append($"<p style=\"margin: 0 0 8px 0;\">{Escape(e.Description).Replace("\n", "<br>")}</p>");
            }

            html.Append($"<p style=\"margin: 0;\"><a href=\"{Escape(url)}\">Veure l'esdeveniment</a></p>");
            html.Append("</div>");
        }

        html.Append("<p style=\"color: #888; font-size: 12px;\">Correu automàtic generat per l'aplicació de pagaments.</p>");
        html.Append("</body></html>");

        return html.ToString();
    }

    private string TimeRange(Event e)
    {
        DateTimeOffset start = TimeZoneInfo.ConvertTime(e.Date, timeZone);
        if (!e.EndDate.HasValue) return $"A partir de les {Format(start, "HH:mm")}h";

        DateTimeOffset end = TimeZoneInfo.ConvertTime(e.EndDate.Value, timeZone);
        if (DateOnly.FromDateTime(end.DateTime) != DateOnly.FromDateTime(start.DateTime))
        {
            return $"Del {Format(start, "dd/MM/yyyy HH:mm")}h al {Format(end, "dd/MM/yyyy HH:mm")}h";
        }

        return $"De les {Format(start, "HH:mm")}h a les {Format(end, "HH:mm")}h";
    }

    private static string FormatDate(DateOnly day) => day.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    private static string Format(DateTimeOffset value, string format) => value.ToString(format, CultureInfo.InvariantCulture);

    private static string Escape(string value) => WebUtility.HtmlEncode(value);
}
