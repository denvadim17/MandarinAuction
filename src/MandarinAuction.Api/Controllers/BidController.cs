using MandarinAuction.Api.Extensions;
using MandarinAuction.Api.Hubs;
using MandarinAuction.Application.Abstractions.Services;
using MandarinAuction.Application.Bid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MandarinAuction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BidController : ControllerBase
{
    private readonly IBidService _bidService;
    private readonly IHubContext<AuctionHub> _hub;

    public BidController(IBidService bidService, IHubContext<AuctionHub> hub)
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

        var result = await _bidService.PlaceBidAsync(userId, request.MandarinId, request.Amount);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        await _hub.Clients.All.SendAsync(AuctionHub.ClientEvents.BidPlaced, result.Bid);
        return Ok(new { message = result.Message, bid = result.Bid });
    }

    [HttpPost("buy-now/{mandarinId:guid}")]
    public async Task<ActionResult> BuyNow(Guid mandarinId)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "В токене нет id пользователя" });
        var result = await _bidService.BuyNowAsync(userId, mandarinId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        await _hub.Clients.All.SendAsync(AuctionHub.ClientEvents.MandarinSold, new { MandarinId = mandarinId });
        return Ok(new { message = result.Message });
    }

    [HttpGet("{mandarinId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<BidDto>>> GetBids(Guid mandarinId)
    {
        var bids = await _bidService.GetBidsForMandarinAsync(mandarinId);
        return Ok(bids);
    }
}
