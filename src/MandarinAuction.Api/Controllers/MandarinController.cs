using MandarinAuction.Application.Abstractions.Services;
using MandarinAuction.Application.Mandarin;
using Microsoft.AspNetCore.Mvc;

namespace MandarinAuction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MandarinController : ControllerBase
{
    private readonly IMandarinService _mandarinService;

    public MandarinController(IMandarinService mandarinService)
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
