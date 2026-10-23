using Application.Common;
using Application.Events.Commands;
using MediatR;

namespace WebServer.Services;

/// <summary>
/// Cada pocs minuts envia a extraescolars els avisos d'obertura i tancament del pagament dels events.
/// </summary>
public class EventPublicationEmailService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventPublicationEmailService> _logger;
    private readonly bool enabled;
    private readonly TimeSpan interval;

    public EventPublicationEmailService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<EventPublicationEmailService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        enabled = configuration.GetValue<bool?>("EventPublicationEmailEnabled") ?? false;
        interval = TimeSpan.FromMinutes(configuration.GetValue<int?>("EventPublicationEmailIntervalMinutes") ?? 5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!enabled)
        {
            _logger.LogInformation("Avisos de publicació d'events desactivats (EventPublicationEmailEnabled)");
            return;
        }

        using PeriodicTimer timer = new PeriodicTimer(interval);
        do
        {
            await SendAsync(stoppingToken);
        }
        while (await WaitAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            return await timer.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task SendAsync(CancellationToken ct)
    {
        try
        {
            // El servei és singleton i el mediator i els repositoris són scoped.
            using IServiceScope scope = _scopeFactory.CreateScope();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var response = await mediator.Send(new SendEventPublicationEmailsCommand(), ct);
            if (response.Code != ResponseCode.Success)
            {
                _logger.LogError("Error enviant els avisos de publicació d'events: {Errors}",
                    string.Join("; ", response.Errors?.SelectMany(x => x.Value) ?? Array.Empty<string>()));
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (System.Exception e)
        {
            // Els avisos no marcats es tornaran a intentar a la propera volta.
            _logger.LogError(e, "Error inesperat enviant els avisos de publicació d'events");
        }
    }
}
