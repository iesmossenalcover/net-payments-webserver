using Domain.Entities.Orders;
using Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.BackgroundServices;

public class FailedOrdersService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FailedOrdersService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public FailedOrdersService(IServiceProvider serviceProvider, ILogger<FailedOrdersService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("FailedOrdersService started at: {time}", DateTimeOffset.Now);
        while (!ct.IsCancellationRequested)
        {
            _logger.LogInformation("FailedOrdersService execution at: {time}", DateTimeOffset.Now);
            using var scope = _serviceProvider.CreateScope();
            var ordersRepository = scope.ServiceProvider.GetRequiredService<IOrdersRepository>();
            var redsys = scope.ServiceProvider.GetRequiredService<IRedsys>();

            var recentThreshold = DateTimeOffset.UtcNow.AddMinutes(-30);
            var pendingOrders = await ordersRepository.GetAllAsync(ct); // TODO: Use a more efficient query

            foreach (var order in pendingOrders)
            {
                //redsys.
            }

            await Task.Delay(_interval, ct);
        }
        _logger.LogInformation("FailedOrdersService stopped at: {time}", DateTimeOffset.Now);
    }
}