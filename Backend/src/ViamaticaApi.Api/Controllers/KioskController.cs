using Microsoft.AspNetCore.Mvc;
using ViamaticaApi.Application.DTOs.Kiosk;
using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Api.Controllers;

[ApiController]
[Route("api/kiosk")]
public class KioskController : ControllerBase
{
    private readonly IKioskService _kioskService;
    private readonly IAttentionService _attentionService;

    public KioskController(IKioskService kioskService, IAttentionService attentionService)
    {
        _kioskService = kioskService;
        _attentionService = attentionService;
    }

    [HttpGet("check/{identification}")]
    public async Task<IActionResult> CheckClient(string identification)
    {
        var client = await _kioskService.FindClientByIdentificationAsync(identification);

        if (client == null)
            return NotFound(new { message = "Cliente no encontrado." });

        return Ok(client);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] KioskRegisterClientDto dto)
    {
        try
        {
            var result = await _kioskService.RegisterClientAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("request-turn")]
    public async Task<IActionResult> RequestTurn([FromBody] KioskRequestTurnDto dto)
    {
        try
        {
            var result = await _kioskService.RequestTurnAsync(dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
