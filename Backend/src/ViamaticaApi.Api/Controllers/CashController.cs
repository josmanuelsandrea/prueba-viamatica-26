using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViamaticaApi.Application.DTOs.Cash;
using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Api.Controllers;

[ApiController]
[Route("api/cash")]
[Authorize]
public class CashController : ControllerBase
{
    private readonly ICashService _cashService;
    private readonly IEncryptionService _encryption;

    public CashController(ICashService cashService, IEncryptionService encryption)
    {
        _cashService = cashService;
        _encryption = encryption;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCashDto dto)
    {
        if (GetCurrentRolId() != 1)
            return Forbid();

        try
        {
            var result = await _cashService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.CashId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _cashService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _cashService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCashDto dto)
    {
        if (GetCurrentRolId() != 1)
            return Forbid();

        try
        {
            var result = await _cashService.UpdateAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (GetCurrentRolId() != 1)
            return Forbid();

        try
        {
            await _cashService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/assign-user")]
    public async Task<IActionResult> AssignUser(int id, [FromBody] AssignUserCashDto dto)
    {
        if (GetCurrentRolId() != 2)
            return Forbid();

        try
        {
            var result = await _cashService.AssignUserAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}/remove-user/{userId:int}")]
    public async Task<IActionResult> RemoveUser(int id, int userId)
    {
        if (GetCurrentRolId() != 2)
            return Forbid();

        try
        {
            await _cashService.RemoveUserAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/activate-user/{userId:int}")]
    public async Task<IActionResult> ActivateUser(int id, int userId)
    {
        if (GetCurrentRolId() != 2)
            return Forbid();

        try
        {
            var result = await _cashService.ActivateUserAsync(id, userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/deactivate-user/{userId:int}")]
    public async Task<IActionResult> DeactivateUser(int id, int userId)
    {
        if (GetCurrentRolId() != 2)
            return Forbid();

        try
        {
            var result = await _cashService.DeactivateUserAsync(id, userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private int GetCurrentRolId()
    {
        var rolIdClaim = User.FindFirst("rolid")?.Value ?? string.Empty;
        return int.Parse(_encryption.Decrypt(rolIdClaim));
    }
}
