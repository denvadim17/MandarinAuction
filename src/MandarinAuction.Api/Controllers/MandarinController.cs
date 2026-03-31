using MandarinAuction.Api.DTOs.Mandarin;
using MandarinAuction.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MandarinAuction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MandarinController : ControllerBase
{
    private readonly MandarinService _mandarinService;

    public MandarinController(MandarinService mandarinService)
    {
        _mandarinService = mandarinService;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<MandarinDto>>> GetActive()
    {
        var mandarins = await _mandarinService.GetActiveMandarinsAsync();
        return Ok(mandarins);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MandarinDto>> GetById(Guid id)
    {
        var mandarin = await _mandarinService.GetMandarinAsync(id);
        return mandarin is null ? NotFound() : Ok(mandarin);
    }
}
