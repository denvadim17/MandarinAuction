using MandarinAuction.Domain.Enums;

namespace MandarinAuction.Application.Mandarin;

public record MandarinDto(
    Guid Id,
    string Name,
    string ImageUrl,
    decimal StartingPrice,
    decimal CurrentPrice,
    decimal BuyNowPrice,
    MandarinStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    int BidCount,
    string? HighestBidderName);
