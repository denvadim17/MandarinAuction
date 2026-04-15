using MandarinAuction.Api.Hubs;
using MandarinAuction.Application.Abstractions.Services;
using Microsoft.AspNetCore.SignalR;

namespace MandarinAuction.Api.BackgroundServices;

/// <summary>Создание мандаринки.</summary>
public class MandarinGeneratorService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IHubContext<AuctionHub> _hub;
    private readonly ILogger<MandarinGeneratorService> _logger;

    public MandarinGeneratorService(
        IServiceProvider services,
        IHubContext<AuctionHub> hub,
        ILogger<MandarinGeneratorService> logger)
    {
        _services = services;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var mandarinService = scope.ServiceProvider.GetRequiredService<IMandarinService>();
                var settingsService = scope.ServiceProvider.GetRequiredService<IAuctionSettingsService>();
                var settings = await settingsService.GetSettingsAsync(stoppingToken);

                var mandarin = await mandarinService.GenerateMandarinAsync();
                await _hub.Clients.All.SendAsync(AuctionHub.ClientEvents.NewMandarin, mandarin, stoppingToken);

                _logger.LogInformation("Добавлена мандаринка: {Name}", mandarin.Name);

                await Task.Delay(TimeSpan.FromMinutes(settings.GenerationIntervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации мандаринки");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
