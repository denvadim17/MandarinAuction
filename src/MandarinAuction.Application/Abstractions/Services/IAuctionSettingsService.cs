using MandarinAuction.Application.Admin;

namespace MandarinAuction.Application.Abstractions.Services;

public interface IAuctionSettingsService
{
    Task<AuctionSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<AuctionSettingsDto> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken cancellationToken = default);
}
