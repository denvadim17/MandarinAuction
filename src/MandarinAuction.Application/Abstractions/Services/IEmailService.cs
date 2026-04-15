namespace MandarinAuction.Application.Abstractions.Services;

public interface IEmailService
{
    Task SendBidOutbidNotificationAsync(string toEmail, string userName, string mandarinName, decimal newBid);
    Task SendPurchaseReceiptAsync(string toEmail, string userName, string mandarinName, decimal price, decimal cashback);
    Task SendAuctionWonAsync(string toEmail, string userName, string mandarinName, decimal winningBid);
}
