using System.Globalization;
using System.Net;
using System.Text;
using Application.Common;
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
    private readonly IEventsPeopleRespository _eventsPeopleRepository;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepository;
    private readonly ICoursesRepository _coursesRepository;
    private readonly ILogger<SendDailyEventsEmailCommandHandler> _logger;
    private readonly string recipient;
    private readonly string frontEventSummaryUrl;
    private readonly TimeZoneInfo timeZone;

    public SendDailyEventsEmailCommandHandler(
        IGoogleAdminApi googleAdminApi,
        IEventsRespository eventsRepository,
        IEventsPeopleRespository eventsPeopleRepository,
        IPersonGroupCourseRepository personGroupCourseRepository,
        ICoursesRepository coursesRepository,
        IConfiguration configuration,
        ILogger<SendDailyEventsEmailCommandHandler> logger)
    {
        _googleAdminApi = googleAdminApi;
        _eventsRepository = eventsRepository;
        _eventsPeopleRepository = eventsPeopleRepository;
        _personGroupCourseRepository = personGroupCourseRepository;
        _coursesRepository = coursesRepository;
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

        Course course = await _coursesRepository.GetCurrentCoursAsync(ct);

        List<EventSummary> summaries = new List<EventSummary>();
        foreach (Event e in excursions)
        {
            IEnumerable<EventPerson> eventPeople = await _eventsPeopleRepository.GetAllByEventIdAsync(e.Id, ct);
            long[] peopleIds = eventPeople.Select(x => x.PersonId).Distinct().ToArray();

            IEnumerable<PersonGroupCourse> peopleGroups = peopleIds.Length == 0
                ? Array.Empty<PersonGroupCourse>()
                : await _personGroupCourseRepository.GetPeopleGroupByPeopleIdsAndCourseIdAsync(course.Id, peopleIds, ct);

            List<string> groups = peopleGroups
                .Select(x => x.Group.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            summaries.Add(new EventSummary(e, groups));
        }

        string subject = $"[EXTRAESCOLARS] Activitats d'avui {FormatDate(day)}";
        string body = BuildBody(day, summaries);

        var result = await _googleAdminApi.SendHtmlEmail(recipient, subject, body, ct);
        if (!result.Success)
        {
            return Response<SendDailyEventsEmailCommandVm>.Error(ResponseCode.InternalError, result.ErrorMessage ?? "Error enviant el resum diari d'events");
        }

        return Response<SendDailyEventsEmailCommandVm>.Ok(new SendDailyEventsEmailCommandVm(true, summaries.Count, result.Data));
    }

    private DateTimeOffset DayStart(DateOnly day)
    {
        DateTime local = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, timeZone.GetUtcOffset(local));
    }

    private string BuildBody(DateOnly day, IEnumerable<EventSummary> summaries)
    {
        StringBuilder html = new StringBuilder();
        html.Append("<html><body style=\"font-family: Arial, Helvetica, sans-serif; color: #222;\">");
        html.Append($"<p>Activitats previstes per a avui, {Escape(FormatDate(day))}:</p>");

        foreach (EventSummary s in summaries)
        {
            string url = frontEventSummaryUrl.Replace("{code}", Uri.EscapeDataString(s.Event.Code));

            html.Append("<div style=\"margin: 0 0 24px 0; padding: 12px 16px; border-left: 4px solid #d81b60;\">");
            html.Append($"<h3 style=\"margin: 0 0 8px 0;\">{Escape(s.Event.Name)}</h3>");
            html.Append($"<p style=\"margin: 0 0 8px 0; color: #555;\">{Escape(TimeRange(s.Event))}</p>");

            if (!string.IsNullOrWhiteSpace(s.Event.Description))
            {
                html.Append($"<p style=\"margin: 0 0 8px 0;\">{Escape(s.Event.Description).Replace("\n", "<br>")}</p>");
            }

            if (s.Groups.Count == 0)
            {
                html.Append("<p style=\"margin: 0 0 8px 0;\"><strong>Grups afectats:</strong> cap alumne apuntat.</p>");
            }
            else
            {
                html.Append("<p style=\"margin: 0 0 4px 0;\"><strong>Grups afectats:</strong></p>");
                html.Append("<ul style=\"margin: 0 0 8px 0;\">");
                foreach (string g in s.Groups)
                {
                    html.Append($"<li>{Escape(g)}</li>");
                }
                html.Append("</ul>");
            }

            html.Append($"<p style=\"margin: 0;\"><a href=\"{Escape(url)}\">Veure l'event</a></p>");
            html.Append("</div>");
        }

        html.Append("<p style=\"color: #888; font-size: 12px;\">Correu automàtic generat per l'aplicació de pagaments.</p>");
        html.Append("</body></html>");

        return html.ToString();
    }

    private string TimeRange(Event e)
    {
        DateTimeOffset start = TimeZoneInfo.ConvertTime(e.Date, timeZone);
        if (!e.EndDate.HasValue) return $"A partir de les {Format(start, "HH:mm")}";

        DateTimeOffset end = TimeZoneInfo.ConvertTime(e.EndDate.Value, timeZone);
        if (DateOnly.FromDateTime(end.DateTime) != DateOnly.FromDateTime(start.DateTime))
        {
            return $"Del {Format(start, "dd/MM/yyyy HH:mm")} al {Format(end, "dd/MM/yyyy HH:mm")}";
        }

        return $"De les {Format(start, "HH:mm")} a les {Format(end, "HH:mm")}";
    }

    private static string FormatDate(DateOnly day) => day.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    private static string Format(DateTimeOffset value, string format) => value.ToString(format, CultureInfo.InvariantCulture);

    private static string Escape(string value) => WebUtility.HtmlEncode(value);

    private record EventSummary(Event Event, List<string> Groups);
}
