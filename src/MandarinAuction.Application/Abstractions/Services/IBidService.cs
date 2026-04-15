using MandarinAuction.Application.Bid;

namespace MandarinAuction.Application.Abstractions.Services;

public interface IBidService
{
    Task<PlaceBidResult> PlaceBidAsync(string userId, Guid mandarinId, decimal amount, CancellationToken cancellationToken = default);
    Task<PurchaseResult> BuyNowAsync(string userId, Guid mandarinId, CancellationToken cancellationToken = default);
    Task<List<BidDto>> GetBidsForMandarinAsync(Guid mandarinId, CancellationToken cancellationToken = default);
}
