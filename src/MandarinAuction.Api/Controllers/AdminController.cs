using MandarinAuction.Application.Abstractions.Services;
using MandarinAuction.Application.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MandarinAuction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAuctionSettingsService _settingsService;
    private readonly IMandarinService _mandarinService;

    public AdminController(IAuctionSettingsService settingsService, IMandarinService mandarinService)
    {
        _settingsService = settingsService;
        _mandarinService = mandarinService;
    }

    [HttpGet("settings")]
    public async Task<ActionResult> GetSettings()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return Ok(settings);
    }

    [HttpPut("settings")]
    public async Task<ActionResult> UpdateSettings(UpdateSettingsRequest request)
    {
        var settings = await _settingsService.UpdateSettingsAsync(request);
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
