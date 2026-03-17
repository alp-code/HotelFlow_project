using HotelFlow.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HotelFlow.API.Services;

public class ExpiredTasksBackgroundService : BackgroundService
{
    private readonly ILogger<ExpiredTasksBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15); // Provera svakih 15 min

    public ExpiredTasksBackgroundService(
        ILogger<ExpiredTasksBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expired Tasks Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var housekeepingService = scope.ServiceProvider.GetRequiredService<IHousekeepingService>();
                    await housekeepingService.HandleExpiredTasksAsync();
                }

                _logger.LogInformation("Checked for expired tasks.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking expired tasks.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Expired Tasks Background Service is stopping.");
    }
}