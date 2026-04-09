using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViamaticaApi.Application.DTOs.Attentions;
using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Api.Controllers;

[ApiController]
[Route("api/attentions")]
[Authorize]
public class AttentionsController : ControllerBase
{
    private readonly IAttentionService _attentionService;
    private readonly IEncryptionService _encryption;

    public AttentionsController(IAttentionService attentionService, IEncryptionService encryption)
    {
        _attentionService = attentionService;
        _encryption = encryption;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAttentionDto dto)
    {
        try
        {
            var result = await _attentionService.CreateAsync(dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAttentionStatusDto dto)
    {
        try
        {
            var result = await _attentionService.UpdateStatusAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? cashId, [FromQuery] DateTime? date)
    {
        var result = await _attentionService.GetAllAsync(cashId, date);
        return Ok(result);
    }

    [HttpGet("daily-summary")]
    public async Task<IActionResult> GetDailySummary()
    {
        var userId = int.Parse(_encryption.Decrypt(User.FindFirst("userid")?.Value ?? string.Empty));
        var rolId = int.Parse(_encryption.Decrypt(User.FindFirst("rolid")?.Value ?? string.Empty));
        var result = await _attentionService.GetDailySummaryAsync(userId, rolId);
        return Ok(result);
    }
}
