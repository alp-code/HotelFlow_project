using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HotelFlow.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelFlow.API.BackgroundServices;

public class NoShowBackgroundService : BackgroundService
{
    private readonly ILogger<NoShowBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public NoShowBackgroundService(
        ILogger<NoShowBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NoShow Background Service is running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;

                // Pokreni svaki dan u 02:00 AM
                var nextRun = now.Date.AddDays(1).AddHours(2);
                var delay = nextRun - now;

                _logger.LogInformation($"Next no-show check at: {nextRun}");

                await Task.Delay(delay, stoppingToken);

                // Procesuiraj no-show-ove
                using var scope = _serviceProvider.CreateScope();
                var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

                await reservationService.ProcessAutomaticNoShowsAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred in NoShowBackgroundService");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}