using MandarinAuction.Api.Data;
using MandarinAuction.Api.Domain.Entities;
using MandarinAuction.Api.Domain.Enums;
using MandarinAuction.Api.DTOs.Mandarin;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Api.Services;

public class MandarinService
{
    private readonly AppDbContext _db;
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

    public MandarinService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Mandarin> GenerateMandarinAsync()
    {
        var settings = await _db.AuctionSettings.FirstAsync();
        var rnd = Random.Shared;

        var basePrice = Math.Round((decimal)(rnd.NextDouble() * 9 + 1), 2);

        var mandarin = new Mandarin
        {
            Name = Names[rnd.Next(Names.Length)],
            ImageUrl = ImageUrls[rnd.Next(ImageUrls.Length)],
            StartingPrice = basePrice,
            CurrentPrice = basePrice,
            BuyNowPrice = Math.Round(basePrice * (decimal)(rnd.NextDouble() * 2 + 2), 2),
            ExpiresAt = DateTime.UtcNow.AddMinutes(settings.MandarinLifetimeMinutes)
        };

        _db.Mandarins.Add(mandarin);
        await _db.SaveChangesAsync();
        return mandarin;
    }

    // SQLite плохо переводит сортировку по decimal внутри подзапроса, поэтому Include + ToDto в памяти
    public async Task<List<MandarinDto>> GetActiveMandarinsAsync()
    {
        var mandarins = await _db.Mandarins
            .AsNoTracking()
            .Where(m => m.Status == MandarinStatus.Active && m.ExpiresAt > DateTime.UtcNow)
            .Include(m => m.Bids).ThenInclude(b => b.User)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return mandarins.Select(ToDto).ToList();
    }

    public async Task<MandarinDto?> GetMandarinAsync(Guid id)
    {
        var m = await _db.Mandarins
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Include(x => x.Bids).ThenInclude(b => b.User)
            .FirstOrDefaultAsync();

        return m is null ? null : ToDto(m);
    }

    private static MandarinDto ToDto(Mandarin m)
    {
        var top = m.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
        return new MandarinDto(
            m.Id, m.Name, m.ImageUrl,
            m.StartingPrice, m.CurrentPrice, m.BuyNowPrice,
            m.Status,
            m.CreatedAt, m.ExpiresAt,
            m.Bids.Count,
            top?.User?.UserName);
    }

    public async Task<int> CleanupSpoiledMandarinsAsync()
    {
        var spoiled = await _db.Mandarins
            .Where(m => m.Status == MandarinStatus.Active && m.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var m in spoiled)
            m.Status = MandarinStatus.Spoiled;

        await _db.SaveChangesAsync();
        return spoiled.Count;
    }

    // Время вышло: если были ставки — продаём победителю, иначе мандаринка «испортилась»
    public async Task<List<(Mandarin Mandarin, Bid WinningBid)>> FinalizeExpiredAuctionsAsync()
    {
        var expired = await _db.Mandarins
            .Include(m => m.Bids).ThenInclude(b => b.User)
            .Where(m => m.Status == MandarinStatus.Active && m.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        var winners = new List<(Mandarin, Bid)>();

        foreach (var m in expired)
        {
            var topBid = m.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
            if (topBid is not null)
            {
                m.Status = MandarinStatus.Sold;
                m.WinnerId = topBid.UserId;
                topBid.User.TotalPurchasedMandarins++;
                winners.Add((m, topBid));
            }
            else
            {
                m.Status = MandarinStatus.Spoiled;
            }
        }

        await _db.SaveChangesAsync();
        return winners;
    }
}
