namespace MandarinAuction.Application.Admin;

public record AuctionSettingsDto(
    int Id,
    decimal CashbackBasePercent,
    decimal CashbackPerMandarinBonus,
    decimal CashbackMaxPercent,
    int MandarinLifetimeMinutes,
    int GenerationIntervalMinutes);
