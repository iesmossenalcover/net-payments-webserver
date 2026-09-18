using System.Globalization;
using Application.Common;
using Application.Events.Commands;
using MediatR;

namespace WebServer.Services;

/// <summary>
/// Cada dia, a l'hora configurada, envia al claustre el resum dels events que comencen avui.
/// </summary>
public class DailyEventsEmailService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyEventsEmailService> _logger;
    private readonly bool enabled;
    private readonly TimeOnly sendAt;
    private readonly TimeZoneInfo timeZone;

    public DailyEventsEmailService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<DailyEventsEmailService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        enabled = configuration.GetValue<bool?>("DailyEventsEmailEnabled") ?? false;
        sendAt = TimeOnly.Parse(configuration.GetValue<string>("DailyEventsEmailTime") ?? "08:00", CultureInfo.InvariantCulture);
        timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration.GetValue<string>("DailyEventsEmailTimeZone") ?? "Europe/Madrid");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!enabled)
        {
            _logger.LogInformation("Resum diari d'events desactivat (DailyEventsEmailEnabled)");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            DateTimeOffset now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            DateTimeOffset next = NextRun(now);

            _logger.LogInformation("Pròxim resum diari d'events: {Next}", next);

            try
            {
                await Task.Delay(next - now, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            await SendAsync(stoppingToken);
        }
    }

    private async Task SendAsync(CancellationToken ct)
    {
        try
        {
            // El servei és singleton i el mediator i els repositoris són scoped.
            using IServiceScope scope = _scopeFactory.CreateScope();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var response = await mediator.Send(new SendDailyEventsEmailCommand(), ct);
            if (response.Code != ResponseCode.Success)
            {
                _logger.LogError("Error enviant el resum diari d'events: {Errors}",
                    string.Join("; ", response.Errors?.SelectMany(x => x.Value) ?? Array.Empty<string>()));
            }
        }
        catch (System.Exception e)
        {
            // Una errada puntual no ha de tombar el servei: demà s'ha de tornar a intentar.
            _logger.LogError(e, "Error inesperat enviant el resum diari d'events");
        }
    }

    private DateTimeOffset NextRun(DateTimeOffset now)
    {
        DateOnly day = DateOnly.FromDateTime(now.DateTime);
        DateTimeOffset todayRun = AtSendTime(day);

        return todayRun > now ? todayRun : AtSendTime(day.AddDays(1));
    }

    private DateTimeOffset AtSendTime(DateOnly day)
    {
        DateTime local = day.ToDateTime(sendAt, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, timeZone.GetUtcOffset(local));
    }
}
