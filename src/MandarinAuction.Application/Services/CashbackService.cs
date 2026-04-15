using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Application.Abstractions.Services;

namespace MandarinAuction.Application.Services;

public class CashbackService : ICashbackService
{
    private readonly IAuctionSettingsRepository _settingsRepository;
    private readonly IUserRepository _userRepository;

    public CashbackService(IAuctionSettingsRepository settingsRepository, IUserRepository userRepository)
    {
        _settingsRepository = settingsRepository;
        _userRepository = userRepository;
    }

    public async Task<decimal> CalculateCashbackPercentAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetAsync(cancellationToken);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return 0;

        var percent = settings.CashbackBasePercent
                      + user.TotalPurchasedMandarins * settings.CashbackPerMandarinBonus;

        return Math.Min(percent, settings.CashbackMaxPercent);
    }

    public async Task<decimal> CalculateCashbackAmountAsync(
        string userId,
        decimal purchaseAmount,
        CancellationToken cancellationToken = default)
    {
        var percent = await CalculateCashbackPercentAsync(userId, cancellationToken);
        return Math.Round(purchaseAmount * percent / 100m, 2);
    }
}
