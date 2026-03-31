using MandarinAuction.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Api.Services;

public class CashbackService
{
    private readonly AppDbContext _db;

    public CashbackService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> CalculateCashbackPercentAsync(string userId)
    {
        var settings = await _db.AuctionSettings.FirstAsync();
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return 0;

        var percent = settings.CashbackBasePercent
                      + user.TotalPurchasedMandarins * settings.CashbackPerMandarinBonus;

        return Math.Min(percent, settings.CashbackMaxPercent);
    }

    public async Task<decimal> CalculateCashbackAmountAsync(string userId, decimal purchaseAmount)
    {
        var percent = await CalculateCashbackPercentAsync(userId);
        return Math.Round(purchaseAmount * percent / 100m, 2);
    }
}
