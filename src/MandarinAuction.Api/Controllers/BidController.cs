using MandarinAuction.Api.DTOs.Bid;
using MandarinAuction.Api.Extensions;
using MandarinAuction.Api.Hubs;
using MandarinAuction.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MandarinAuction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BidController : ControllerBase
{
    private readonly BidService _bidService;
    private readonly IHubContext<AuctionHub> _hub;

    public BidController(BidService bidService, IHubContext<AuctionHub> hub)
    {
        _bidService = bidService;
        _hub = hub;
    }

    [HttpPost]
    public async Task<ActionResult> PlaceBid(PlaceBidRequest request)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "В токене нет id пользователя" });

        var (success, message, bid) = await _bidService.PlaceBidAsync(userId, request.MandarinId, request.Amount);

        if (!success)
            return BadRequest(new { message });

        await _hub.Clients.All.SendAsync(AuctionHub.ClientEvents.BidPlaced, bid);
        return Ok(new { message, bid });
    }

    [HttpPost("buy-now/{mandarinId:guid}")]
    public async Task<ActionResult> BuyNow(Guid mandarinId)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "В токене нет id пользователя" });
        var (success, message) = await _bidService.BuyNowAsync(userId, mandarinId);

        if (!success)
            return BadRequest(new { message });

        await _hub.Clients.All.SendAsync(AuctionHub.ClientEvents.MandarinSold, new { MandarinId = mandarinId });
        return Ok(new { message });
    }

    [HttpGet("{mandarinId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<BidDto>>> GetBids(Guid mandarinId)
    {
        var bids = await _bidService.GetBidsForMandarinAsync(mandarinId);
        return Ok(bids);
    }
}
