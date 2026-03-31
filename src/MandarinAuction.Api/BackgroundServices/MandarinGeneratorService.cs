using MandarinAuction.Api.Data;
using MandarinAuction.Api.DTOs.Mandarin;
using MandarinAuction.Api.Hubs;
using MandarinAuction.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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
                var mandarinService = scope.ServiceProvider.GetRequiredService<MandarinService>();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var settings = await db.AuctionSettings.FirstAsync(stoppingToken);

                var mandarin = await mandarinService.GenerateMandarinAsync();

                var dto = new MandarinDto(
                    mandarin.Id, mandarin.Name, mandarin.ImageUrl,
                    mandarin.StartingPrice, mandarin.CurrentPrice, mandarin.BuyNowPrice,
                    mandarin.Status,
                    mandarin.CreatedAt, mandarin.ExpiresAt, 0, null
                );
                await _hub.Clients.All.SendAsync(AuctionHub.ClientEvents.NewMandarin, dto, stoppingToken);

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
