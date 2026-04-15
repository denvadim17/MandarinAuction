using MandarinAuction.Domain.Enums;

namespace MandarinAuction.Application.Abstractions.Services;

public interface IWalletService
{
    Task<decimal> GetBalanceAsync(string userId, CancellationToken cancellationToken = default);
    Task<(decimal Balance, int TotalPurchased)> GetWalletSummaryAsync(string userId, CancellationToken cancellationToken = default);
    Task DepositAsync(string userId, decimal amount, CancellationToken cancellationToken = default);
    Task<bool> DeductAsync(string userId, decimal amount, TransactionType type, string description, CancellationToken cancellationToken = default);
    Task CreditAsync(string userId, decimal amount, TransactionType type, string description, CancellationToken cancellationToken = default);
    string GeneratePaymentLink(string userId, decimal amount);
}
