using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Application.Abstractions.Services;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Domain.Enums;

namespace MandarinAuction.Application.Services;

public class WalletService : IWalletService
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletService(
        IUserRepository userRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> GetBalanceAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId, cancellationToken);
        return user.Balance;
    }

    public async Task<(decimal Balance, int TotalPurchased)> GetWalletSummaryAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId, cancellationToken);
        return (user.Balance, user.TotalPurchasedMandarins);
    }

    public async Task DepositAsync(string userId, decimal amount, CancellationToken cancellationToken = default)
    {
        await CreditAsync(userId, amount, TransactionType.Deposit, "Пополнение кошелька", cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeductAsync(
        string userId,
        decimal amount,
        TransactionType type,
        string description,
        CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId, cancellationToken);
        if (user.Balance < amount)
            return false;

        user.Balance -= amount;

        await _transactionRepository.AddAsync(
            new Transaction
            {
                UserId = userId,
                Amount = -amount,
                Type = type,
                Description = description
            },
            cancellationToken);

        return true;
    }

    public async Task CreditAsync(
        string userId,
        decimal amount,
        TransactionType type,
        string description,
        CancellationToken cancellationToken = default)
    {
        var user = await GetRequiredUserAsync(userId, cancellationToken);
        user.Balance += amount;

        await _transactionRepository.AddAsync(
            new Transaction
            {
                UserId = userId,
                Amount = amount,
                Type = type,
                Description = description
            },
            cancellationToken);
    }

    public string GeneratePaymentLink(string userId, decimal amount)
    {
        return $"/api/wallet/confirm-deposit?userId={userId}&amount={amount}&token={Guid.NewGuid():N}";
    }

    private async Task<AppUser> GetRequiredUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(userId, cancellationToken)
               ?? throw new InvalidOperationException("Пользователь не найден");
    }
}
