using MandarinAuction.Api.Hubs;
using MandarinAuction.Application.Abstractions.Services;
using Microsoft.AspNetCore.SignalR;

namespace MandarinAuction.Api.BackgroundServices;

/// <summary>Раз в час закрывает просроченные аукционы</summary>
public class MandarinCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IHubContext<AuctionHub> _hub;
    private readonly ILogger<MandarinCleanupService> _logger;

    public MandarinCleanupService(
        IServiceProvider services,
        IHubContext<AuctionHub> hub,
        ILogger<MandarinCleanupService> logger)
    {
        _services = services;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var mandarinService = scope.ServiceProvider.GetRequiredService<IMandarinService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var winners = await mandarinService.FinalizeExpiredAuctionsAsync();
                foreach (var winner in winners)
                {
                    await _hub.Clients.All.SendAsync(
                        AuctionHub.ClientEvents.AuctionWon,
                        new { MandarinId = winner.MandarinId, Winner = winner.WinnerName, Amount = winner.WinningBid },
                        stoppingToken);

                    _ = emailService.SendAuctionWonAsync(
                        winner.WinnerEmail,
                        winner.WinnerName,
                        winner.MandarinName,
                        winner.WinningBid);
                }

                var cleaned = await mandarinService.CleanupSpoiledMandarinsAsync();
                if (cleaned > 0)
                {
                    _logger.LogInformation("Помечено испорченных мандаринок: {Count}", cleaned);
                    await _hub.Clients.All.SendAsync(AuctionHub.ClientEvents.MandarinExpired, cleaned, stoppingToken);
                }

                _logger.LogInformation(
                    "Очистка: победителей аукциона {W}, испорченных {C}",
                    winners.Count, cleaned);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка фоновой очистки");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
