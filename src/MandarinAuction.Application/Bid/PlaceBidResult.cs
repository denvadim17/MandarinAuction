namespace MandarinAuction.Application.Bid;

public record PlaceBidResult(bool Success, string Message, BidDto? Bid);
