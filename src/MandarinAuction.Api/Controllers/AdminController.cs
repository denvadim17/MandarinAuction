using MandarinAuction.Api.Data;
using MandarinAuction.Api.DTOs.Admin;
using MandarinAuction.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly MandarinService _mandarinService;

    public AdminController(AppDbContext db, MandarinService mandarinService)
    {
        _db = db;
        _mandarinService = mandarinService;
    }

    [HttpGet("settings")]
    public async Task<ActionResult> GetSettings()
    {
        var settings = await _db.AuctionSettings.FirstAsync();
        return Ok(settings);
    }

    [HttpPut("settings")]
    public async Task<ActionResult> UpdateSettings(UpdateSettingsRequest request)
    {
        var settings = await _db.AuctionSettings.FirstAsync();

        if (request.CashbackBasePercent.HasValue)
            settings.CashbackBasePercent = request.CashbackBasePercent.Value;
        if (request.CashbackPerMandarinBonus.HasValue)
            settings.CashbackPerMandarinBonus = request.CashbackPerMandarinBonus.Value;
        if (request.CashbackMaxPercent.HasValue)
            settings.CashbackMaxPercent = request.CashbackMaxPercent.Value;
        if (request.MandarinLifetimeMinutes.HasValue)
            settings.MandarinLifetimeMinutes = request.MandarinLifetimeMinutes.Value;
        if (request.GenerationIntervalMinutes.HasValue)
            settings.GenerationIntervalMinutes = request.GenerationIntervalMinutes.Value;

        await _db.SaveChangesAsync();
        return Ok(settings);
    }

    [HttpPost("generate-mandarin")]
    public async Task<ActionResult> ForceGenerate()
    {
        var mandarin = await _mandarinService.GenerateMandarinAsync();
        return Ok(mandarin);
    }

    [HttpPost("cleanup")]
    public async Task<ActionResult> ForceCleanup()
    {
        var count = await _mandarinService.CleanupSpoiledMandarinsAsync();
        return Ok(new { cleaned = count });
    }
}
