namespace MandarinAuction.Api.Domain.Entities;

public class AuctionSettings
{
    public int Id { get; set; } = 1;
    public decimal CashbackBasePercent { get; set; } = 5m;
    public decimal CashbackPerMandarinBonus { get; set; } = 0.5m;
    public decimal CashbackMaxPercent { get; set; } = 20m;
    public int MandarinLifetimeMinutes { get; set; } = 1440; // 24 часа
    public int GenerationIntervalMinutes { get; set; } = 60;
}
