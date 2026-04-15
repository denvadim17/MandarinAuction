using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Application.Abstractions.Services;
using MandarinAuction.Application.Mandarin;
using MandarinAuction.Domain.Enums;
using DomainMandarin = MandarinAuction.Domain.Entities.Mandarin;

namespace MandarinAuction.Application.Services;

public class MandarinService : IMandarinService
{
    private readonly IMandarinRepository _mandarinRepository;
    private readonly IAuctionSettingsRepository _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly string[] Names =
    [
        "Солнечная мандаринка", "Сочная красавица", "Оранжевое чудо",
        "Мандарин-великан", "Маленький цитрус", "Золотистая прелесть",
        "Южная гостья", "Новогодняя мандаринка", "Ароматная радость",
        "Мандаринка-счастье", "Императорский мандарин", "Тропическая звезда"
    ];

    private static readonly string[] ImageUrls =
    [
        "https://em-content.zobj.net/source/apple/391/tangerine_1f34a.png",
        "https://em-content.zobj.net/thumbs/240/google/350/tangerine_1f34a.png",
        "https://em-content.zobj.net/thumbs/240/samsung/349/tangerine_1f34a.png",
        "https://em-content.zobj.net/thumbs/240/microsoft/319/tangerine_1f34a.png"
    ];

    public MandarinService(
        IMandarinRepository mandarinRepository,
        IAuctionSettingsRepository settingsRepository,
        IUnitOfWork unitOfWork)
    {
        _mandarinRepository = mandarinRepository;
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MandarinDto> GenerateMandarinAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetAsync(cancellationToken);
        var rnd = Random.Shared;
        var basePrice = Math.Round((decimal)(rnd.NextDouble() * 9 + 1), 2);

        var mandarin = new DomainMandarin
        {
            Name = Names[rnd.Next(Names.Length)],
            ImageUrl = ImageUrls[rnd.Next(ImageUrls.Length)],
            StartingPrice = basePrice,
            CurrentPrice = basePrice,
            BuyNowPrice = Math.Round(basePrice * (decimal)(rnd.NextDouble() * 2 + 2), 2),
            ExpiresAt = DateTime.UtcNow.AddMinutes(settings.MandarinLifetimeMinutes)
        };

        await _mandarinRepository.AddAsync(mandarin, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(mandarin);
    }

    public async Task<List<MandarinDto>> GetActiveMandarinsAsync(CancellationToken cancellationToken = default)
    {
        var mandarins = await _mandarinRepository.GetActiveWithBidsAsync(cancellationToken);
        return mandarins.Select(ToDto).ToList();
    }

    public async Task<MandarinDto?> GetMandarinAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mandarin = await _mandarinRepository.GetByIdWithBidsAsync(id, cancellationToken);
        return mandarin is null ? null : ToDto(mandarin);
    }

    public async Task<int> CleanupSpoiledMandarinsAsync(CancellationToken cancellationToken = default)
    {
        var spoiled = await _mandarinRepository.GetExpiredActiveAsync(cancellationToken);
        foreach (var mandarin in spoiled)
            mandarin.Status = MandarinStatus.Spoiled;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return spoiled.Count;
    }

    public async Task<List<ExpiredAuctionResult>> FinalizeExpiredAuctionsAsync(CancellationToken cancellationToken = default)
    {
        var expired = await _mandarinRepository.GetExpiredActiveWithBidsAsync(cancellationToken);
        var winners = new List<ExpiredAuctionResult>();

        foreach (var mandarin in expired)
        {
            var topBid = mandarin.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
            if (topBid is not null)
            {
                mandarin.Status = MandarinStatus.Sold;
                mandarin.WinnerId = topBid.UserId;
                topBid.User.TotalPurchasedMandarins++;

                if (!string.IsNullOrWhiteSpace(topBid.User.Email))
                {
                    winners.Add(
                        new ExpiredAuctionResult(
                            mandarin.Id,
                            mandarin.Name,
                            topBid.User.UserName ?? "участник",
                            topBid.User.Email!,
                            topBid.Amount));
                }
            }
            else
            {
                mandarin.Status = MandarinStatus.Spoiled;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return winners;
    }

    private static MandarinDto ToDto(DomainMandarin mandarin)
    {
        var topBid = mandarin.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
        return new MandarinDto(
            mandarin.Id,
            mandarin.Name,
            mandarin.ImageUrl,
            mandarin.StartingPrice,
            mandarin.CurrentPrice,
            mandarin.BuyNowPrice,
            mandarin.Status,
            mandarin.CreatedAt,
            mandarin.ExpiresAt,
            mandarin.Bids.Count,
            topBid?.User?.UserName);
    }
}
