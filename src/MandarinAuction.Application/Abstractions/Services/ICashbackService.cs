namespace MandarinAuction.Application.Abstractions.Services;

public interface ICashbackService
{
    Task<decimal> CalculateCashbackPercentAsync(string userId, CancellationToken cancellationToken = default);
    Task<decimal> CalculateCashbackAmountAsync(string userId, decimal purchaseAmount, CancellationToken cancellationToken = default);
}
