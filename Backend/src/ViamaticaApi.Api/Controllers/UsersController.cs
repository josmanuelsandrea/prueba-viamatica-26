using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViamaticaApi.Application.DTOs.Users;
using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEncryptionService _encryption;

    public UsersController(IUserService userService, IEncryptionService encryption)
    {
        _userService = userService;
        _encryption = encryption;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var (creatorId, creatorRolId) = GetCurrentUser();
        if (creatorRolId != 1 && creatorRolId != 2)
            return Forbid();

        try
        {
            var result = await _userService.CreateAsync(dto, creatorId, creatorRolId);
            return CreatedAtAction(nameof(GetById), new { id = result.UserId }, result);
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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? rolId, [FromQuery] string? statusId)
    {
        var (_, rolId_) = GetCurrentUser();
        if (rolId_ != 1 && rolId_ != 2)
            return Forbid();

        var result = await _userService.GetAllAsync(rolId, statusId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var (_, rolId) = GetCurrentUser();
        if (rolId != 1 && rolId != 2)
            return Forbid();

        try
        {
            var result = await _userService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var (_, rolId) = GetCurrentUser();
        if (rolId != 1 && rolId != 2)
            return Forbid();

        try
        {
            var result = await _userService.UpdateAsync(id, dto);
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
        var (creatorId, rolId) = GetCurrentUser();
        if (rolId != 1)
            return Forbid();

        try
        {
            await _userService.DeleteAsync(id, creatorId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var (approvedById, rolId) = GetCurrentUser();
        if (rolId != 1)
            return Forbid();

        try
        {
            var result = await _userService.ApproveAsync(id, approvedById);
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

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeUserStatusDto dto)
    {
        var (_, rolId) = GetCurrentUser();
        if (rolId != 1)
            return Forbid();

        try
        {
            var result = await _userService.ChangeStatusAsync(id, dto);
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

    private (int userId, int rolId) GetCurrentUser()
    {
        var userIdClaim = User.FindFirst("userid")?.Value ?? string.Empty;
        var rolIdClaim = User.FindFirst("rolid")?.Value ?? string.Empty;
        var userId = int.Parse(_encryption.Decrypt(userIdClaim));
        var rolId = int.Parse(_encryption.Decrypt(rolIdClaim));
        return (userId, rolId);
    }
}
