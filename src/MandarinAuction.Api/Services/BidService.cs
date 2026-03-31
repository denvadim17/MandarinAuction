using MandarinAuction.Api.Data;
using MandarinAuction.Api.Domain.Entities;
using MandarinAuction.Api.Domain.Enums;
using MandarinAuction.Api.DTOs.Bid;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Api.Services;

public class BidService
{
    private readonly AppDbContext _db;
    private readonly WalletService _wallet;
    private readonly CashbackService _cashback;
    private readonly EmailService _email;

    public BidService(AppDbContext db, WalletService wallet, CashbackService cashback, EmailService email)
    {
        _db = db;
        _wallet = wallet;
        _cashback = cashback;
        _email = email;
    }

    public async Task<(bool Success, string Message, BidDto? Bid)> PlaceBidAsync(string userId, Guid mandarinId, decimal amount)
    {
        var mandarin = await _db.Mandarins.FirstOrDefaultAsync(m => m.Id == mandarinId);

        if (mandarin is null)
            return (false, "Мандаринка не найдена", null);

        if (mandarin.Status != MandarinStatus.Active)
            return (false, "Аукцион уже завершён", null);

        if (mandarin.ExpiresAt <= DateTime.UtcNow)
            return (false, "Срок мандаринки истёк", null);

        if (amount <= mandarin.CurrentPrice)
            return (false, $"Ставка должна быть выше {mandarin.CurrentPrice}", null);

        var balance = await _wallet.GetBalanceAsync(userId);
        if (balance < amount)
            return (false, "Недостаточно средств на кошельке", null);

        var bidsSnapshot = await _db.Bids
            .Include(b => b.User)
            .Where(b => b.MandarinId == mandarinId)
            .ToListAsync();

        var previousTopBid = bidsSnapshot.OrderByDescending(b => b.Amount).FirstOrDefault();
        
        if (previousTopBid is not null)
        {
            await _wallet.CreditAsync(
                previousTopBid.UserId,
                previousTopBid.Amount,
                TransactionType.BidRelease,
                $"Возврат предыдущей ставки на «{mandarin.Name}»");
        }

        if (!await _wallet.DeductAsync(userId, amount, TransactionType.BidHold, $"Ставка на «{mandarin.Name}»"))
            return (false, "Не удалось списать средства (проверьте баланс)", null);

        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return (false, "Пользователь не найден", null);

        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            MandarinId = mandarinId,
            UserId = userId,
            Amount = amount,
            CreatedAt = DateTime.UtcNow
        };

        _db.Bids.Add(bid);
        mandarin.CurrentPrice = amount;
        await _db.SaveChangesAsync();

        if (previousTopBid is not null && previousTopBid.UserId != userId)
        {
            var outbid = previousTopBid.User ?? await _db.Users.FindAsync(previousTopBid.UserId);
            if (outbid?.Email is { Length: > 0 } email)
            {
                _ = _email.SendBidOutbidNotificationAsync(
                    email,
                    outbid.UserName ?? "участник",
                    mandarin.Name,
                    amount);
            }
        }

        return (true, "Ставка принята", new BidDto(bid.Id, mandarinId, user.UserName!, amount, bid.CreatedAt));
    }

    public async Task<(bool Success, string Message)> BuyNowAsync(string userId, Guid mandarinId)
    {
        var mandarin = await _db.Mandarins
            .Include(m => m.Bids).ThenInclude(b => b.User)
            .FirstOrDefaultAsync(m => m.Id == mandarinId);

        if (mandarin is null)
            return (false, "Мандаринка не найдена");

        if (mandarin.Status != MandarinStatus.Active)
            return (false, "Аукцион уже завершён");

        var balance = await _wallet.GetBalanceAsync(userId);
        if (balance < mandarin.BuyNowPrice)
            return (false, "Недостаточно средств для моментальной покупки");

        var previousTopBid = mandarin.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
        if (previousTopBid is not null)
        {
            await _wallet.CreditAsync(
                previousTopBid.UserId,
                previousTopBid.Amount,
                TransactionType.BidRelease,
                $"Возврат ставки — «{mandarin.Name}» выкуплена");
        }

        if (!await _wallet.DeductAsync(userId, mandarin.BuyNowPrice, TransactionType.Purchase, $"Покупка «{mandarin.Name}»"))
            return (false, "Не удалось списать средства за покупку");

        var cashbackAmount = await _cashback.CalculateCashbackAmountAsync(userId, mandarin.BuyNowPrice);
        if (cashbackAmount > 0)
        {
            await _wallet.CreditAsync(userId, cashbackAmount, TransactionType.Cashback,
                $"Кэшбек за «{mandarin.Name}»");
        }

        mandarin.Status = MandarinStatus.Sold;
        mandarin.WinnerId = userId;

        var user = await _db.Users.FindAsync(userId);
        user!.TotalPurchasedMandarins++;
        await _db.SaveChangesAsync();

        _ = _email.SendPurchaseReceiptAsync(
            user.Email!, user.UserName!, mandarin.Name, mandarin.BuyNowPrice, cashbackAmount);

        return (true, $"Мандаринка «{mandarin.Name}» куплена! Кэшбек: {cashbackAmount:C}");
    }

    public async Task<List<BidDto>> GetBidsForMandarinAsync(Guid mandarinId)
    {
        var bids = await _db.Bids
            .AsNoTracking()
            .Where(b => b.MandarinId == mandarinId)
            .Include(b => b.User)
            .ToListAsync();

        return bids
            .OrderByDescending(b => b.Amount)
            .Select(b => new BidDto(b.Id, b.MandarinId, b.User.UserName!, b.Amount, b.CreatedAt))
            .ToList();
    }
}
