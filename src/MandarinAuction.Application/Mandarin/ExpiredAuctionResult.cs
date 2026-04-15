namespace MandarinAuction.Application.Mandarin;

public record ExpiredAuctionResult(
    Guid MandarinId,
    string MandarinName,
    string WinnerName,
    string WinnerEmail,
    decimal WinningBid);
