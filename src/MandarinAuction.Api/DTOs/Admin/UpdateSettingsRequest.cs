namespace MandarinAuction.Api.DTOs.Admin;

public class UpdateSettingsRequest
{
    public decimal? CashbackBasePercent { get; set; }
    public decimal? CashbackPerMandarinBonus { get; set; }
    public decimal? CashbackMaxPercent { get; set; }
    public int? MandarinLifetimeMinutes { get; set; }
    public int? GenerationIntervalMinutes { get; set; }
}
