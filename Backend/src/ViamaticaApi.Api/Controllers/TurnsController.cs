using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViamaticaApi.Application.DTOs.Turns;
using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Api.Controllers;

[ApiController]
[Route("api/turns")]
[Authorize]
public class TurnsController : ControllerBase
{
    private readonly ITurnService _turnService;
    private readonly IEncryptionService _encryption;

    public TurnsController(ITurnService turnService, IEncryptionService encryption)
    {
        _turnService = turnService;
        _encryption = encryption;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTurnDto dto)
    {
        if (GetCurrentRolId() != 2)
            return Forbid();

        try
        {
            var gestorId = GetCurrentUserId();
            var result = await _turnService.CreateAsync(dto, gestorId);
            return CreatedAtAction(nameof(GetById), new { id = result.TurnId }, result);
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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? cashId, [FromQuery] DateTime? date)
    {
        var result = await _turnService.GetAllAsync(cashId, date);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _turnService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userid")?.Value ?? string.Empty;
        return int.Parse(_encryption.Decrypt(claim));
    }

    private int GetCurrentRolId()
    {
        var claim = User.FindFirst("rolid")?.Value ?? string.Empty;
        return int.Parse(_encryption.Decrypt(claim));
    }
}
