using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Application.Abstractions.Services;
using MandarinAuction.Application.Bid;
using MandarinAuction.Domain.Enums;
using DomainBid = MandarinAuction.Domain.Entities.Bid;

namespace MandarinAuction.Application.Services;

public class BidService : IBidService
{
    private readonly IMandarinRepository _mandarinRepository;
    private readonly IBidRepository _bidRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWalletService _walletService;
    private readonly ICashbackService _cashbackService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public BidService(
        IMandarinRepository mandarinRepository,
        IBidRepository bidRepository,
        IUserRepository userRepository,
        IWalletService walletService,
        ICashbackService cashbackService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _mandarinRepository = mandarinRepository;
        _bidRepository = bidRepository;
        _userRepository = userRepository;
        _walletService = walletService;
        _cashbackService = cashbackService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PlaceBidResult> PlaceBidAsync(
        string userId,
        Guid mandarinId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var mandarin = await _mandarinRepository.GetByIdAsync(mandarinId, cancellationToken);
        if (mandarin is null)
            return new PlaceBidResult(false, "Мандаринка не найдена", null);

        if (mandarin.Status != MandarinStatus.Active)
            return new PlaceBidResult(false, "Аукцион уже завершён", null);

        if (mandarin.ExpiresAt <= DateTime.UtcNow)
            return new PlaceBidResult(false, "Срок мандаринки истёк", null);

        if (amount <= mandarin.CurrentPrice)
            return new PlaceBidResult(false, $"Ставка должна быть выше {mandarin.CurrentPrice}", null);

        var balance = await _walletService.GetBalanceAsync(userId, cancellationToken);
        if (balance < amount)
            return new PlaceBidResult(false, "Недостаточно средств на кошельке", null);

        var bidsSnapshot = await _bidRepository.GetForMandarinWithUsersAsync(mandarinId, cancellationToken);
        var previousTopBid = bidsSnapshot.OrderByDescending(b => b.Amount).FirstOrDefault();

        if (previousTopBid is not null)
        {
            await _walletService.CreditAsync(
                previousTopBid.UserId,
                previousTopBid.Amount,
                TransactionType.BidRelease,
                $"Возврат предыдущей ставки на «{mandarin.Name}»",
                cancellationToken);
        }

        var deducted = await _walletService.DeductAsync(
            userId,
            amount,
            TransactionType.BidHold,
            $"Ставка на «{mandarin.Name}»",
            cancellationToken);

        if (!deducted)
            return new PlaceBidResult(false, "Не удалось списать средства (проверьте баланс)", null);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return new PlaceBidResult(false, "Пользователь не найден", null);

        var bid = new DomainBid
        {
            Id = Guid.NewGuid(),
            MandarinId = mandarinId,
            UserId = userId,
            User = user,
            Amount = amount,
            CreatedAt = DateTime.UtcNow
        };

        await _bidRepository.AddAsync(bid, cancellationToken);
        mandarin.CurrentPrice = amount;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (previousTopBid is not null && previousTopBid.UserId != userId && !string.IsNullOrWhiteSpace(previousTopBid.User?.Email))
        {
            _ = _emailService.SendBidOutbidNotificationAsync(
                previousTopBid.User.Email!,
                previousTopBid.User.UserName ?? "участник",
                mandarin.Name,
                amount);
        }

        return new PlaceBidResult(
            true,
            "Ставка принята",
            new BidDto(bid.Id, mandarinId, user.UserName ?? string.Empty, amount, bid.CreatedAt));
    }

    public async Task<PurchaseResult> BuyNowAsync(
        string userId,
        Guid mandarinId,
        CancellationToken cancellationToken = default)
    {
        var mandarin = await _mandarinRepository.GetByIdWithBidsAsync(mandarinId, cancellationToken);
        if (mandarin is null)
            return new PurchaseResult(false, "Мандаринка не найдена");

        if (mandarin.Status != MandarinStatus.Active)
            return new PurchaseResult(false, "Аукцион уже завершён");

        var balance = await _walletService.GetBalanceAsync(userId, cancellationToken);
        if (balance < mandarin.BuyNowPrice)
            return new PurchaseResult(false, "Недостаточно средств для моментальной покупки");

        var previousTopBid = mandarin.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
        if (previousTopBid is not null)
        {
            await _walletService.CreditAsync(
                previousTopBid.UserId,
                previousTopBid.Amount,
                TransactionType.BidRelease,
                $"Возврат ставки — «{mandarin.Name}» выкуплена",
                cancellationToken);
        }

        var deducted = await _walletService.DeductAsync(
            userId,
            mandarin.BuyNowPrice,
            TransactionType.Purchase,
            $"Покупка «{mandarin.Name}»",
            cancellationToken);

        if (!deducted)
            return new PurchaseResult(false, "Не удалось списать средства за покупку");

        var cashbackAmount = await _cashbackService.CalculateCashbackAmountAsync(userId, mandarin.BuyNowPrice, cancellationToken);
        if (cashbackAmount > 0)
        {
            await _walletService.CreditAsync(
                userId,
                cashbackAmount,
                TransactionType.Cashback,
                $"Кэшбек за «{mandarin.Name}»",
                cancellationToken);
        }

        mandarin.Status = MandarinStatus.Sold;
        mandarin.WinnerId = userId;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return new PurchaseResult(false, "Пользователь не найден");

        user.TotalPurchasedMandarins++;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            _ = _emailService.SendPurchaseReceiptAsync(
                user.Email!,
                user.UserName ?? "участник",
                mandarin.Name,
                mandarin.BuyNowPrice,
                cashbackAmount);
        }

        return new PurchaseResult(true, $"Мандаринка «{mandarin.Name}» куплена! Кэшбек: {cashbackAmount:C}");
    }

    public async Task<List<BidDto>> GetBidsForMandarinAsync(Guid mandarinId, CancellationToken cancellationToken = default)
    {
        var bids = await _bidRepository.GetForMandarinWithUsersAsync(mandarinId, cancellationToken);
        return bids
            .OrderByDescending(b => b.Amount)
            .Select(b => new BidDto(b.Id, b.MandarinId, b.User.UserName ?? string.Empty, b.Amount, b.CreatedAt))
            .ToList();
    }
}
