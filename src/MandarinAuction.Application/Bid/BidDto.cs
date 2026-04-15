namespace MandarinAuction.Application.Bid;

public record BidDto(
    Guid Id,
    Guid MandarinId,
    string UserName,
    decimal Amount,
    DateTime CreatedAt);
