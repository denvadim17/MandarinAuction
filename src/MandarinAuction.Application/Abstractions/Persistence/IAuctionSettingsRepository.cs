using MandarinAuction.Domain.Entities;

namespace MandarinAuction.Application.Abstractions.Persistence;

public interface IAuctionSettingsRepository
{
    Task<AuctionSettings> GetAsync(CancellationToken cancellationToken = default);
}
