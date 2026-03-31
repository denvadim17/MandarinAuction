using Microsoft.AspNetCore.SignalR;

namespace MandarinAuction.Api.Hubs;

public class AuctionHub : Hub
{
    public static class ClientEvents
    {
        public const string NewMandarin = "NewMandarin";
        public const string BidPlaced = "BidPlaced";
        public const string MandarinSold = "MandarinSold";
        public const string MandarinExpired = "MandarinExpired";
        public const string AuctionWon = "AuctionWon";
    }

    public Task JoinAuction(string mandarinId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, mandarinId);

    public Task LeaveAuction(string mandarinId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, mandarinId);
}
