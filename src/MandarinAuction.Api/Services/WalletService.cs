using MandarinAuction.Api.Data;
using MandarinAuction.Api.Domain.Entities;
using MandarinAuction.Api.Domain.Enums;

namespace MandarinAuction.Api.Services;

public class WalletService
{
    private readonly AppDbContext _db;

    public WalletService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<decimal> GetBalanceAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId)
                   ?? throw new InvalidOperationException("Пользователь не найден");
        return user.Balance;
    }

    // Баланс + число покупок 
    public async Task<(decimal Balance, int TotalPurchased)> GetWalletSummaryAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId)
                   ?? throw new InvalidOperationException("Пользователь не найден");
        return (user.Balance, user.TotalPurchasedMandarins);
    }

    public async Task<Transaction> DepositAsync(string userId, decimal amount)
    {
        var user = await _db.Users.FindAsync(userId)
                   ?? throw new InvalidOperationException("Пользователь не найден");

        user.Balance += amount;

        var tx = new Transaction
        {
            UserId = userId,
            Amount = amount,
            Type = TransactionType.Deposit,
            Description = "Пополнение кошелька"
        };
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();
        return tx;
    }

    public async Task<bool> DeductAsync(string userId, decimal amount, TransactionType type, string description)
    {
        var user = await _db.Users.FindAsync(userId)
                   ?? throw new InvalidOperationException("Пользователь не найден");

        if (user.Balance < amount) return false;

        user.Balance -= amount;

        var tx = new Transaction
        {
            UserId = userId,
            Amount = -amount,
            Type = type,
            Description = description
        };
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task CreditAsync(string userId, decimal amount, TransactionType type, string description)
    {
        var user = await _db.Users.FindAsync(userId)
                   ?? throw new InvalidOperationException("Пользователь не найден");

        user.Balance += amount;

        var tx = new Transaction
        {
            UserId = userId,
            Amount = amount,
            Type = type,
            Description = description
        };
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();
    }
    public string GeneratePaymentLink(string userId, decimal amount)
    {
        return $"/api/wallet/confirm-deposit?userId={userId}&amount={amount}&token={Guid.NewGuid():N}";
    }
}
