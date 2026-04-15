using MandarinAuction.Application.Mandarin;

namespace MandarinAuction.Application.Abstractions.Services;

public interface IMandarinService
{
    Task<MandarinDto> GenerateMandarinAsync(CancellationToken cancellationToken = default);
    Task<List<MandarinDto>> GetActiveMandarinsAsync(CancellationToken cancellationToken = default);
    Task<MandarinDto?> GetMandarinAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CleanupSpoiledMandarinsAsync(CancellationToken cancellationToken = default);
    Task<List<ExpiredAuctionResult>> FinalizeExpiredAuctionsAsync(CancellationToken cancellationToken = default);
}
