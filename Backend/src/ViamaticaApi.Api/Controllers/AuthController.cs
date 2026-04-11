using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViamaticaApi.Application.DTOs.Auth;
using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Usuario y contraseña son requeridos." });

        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("recover-password")]
    public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "El correo es requerido." });

        await _authService.RecoverPasswordAsync(request);
        return Ok(new { message = "Si el correo está registrado, recibirás instrucciones para recuperar tu contraseña." });
    }

    [HttpGet("menu")]
    [Authorize]
    public IActionResult GetMenu()
    {
        var rolIdClaim = User.FindFirst("rolid")?.Value;
        if (rolIdClaim == null)
            return Unauthorized(new { message = "Token inválido." });

        var encryptionService = HttpContext.RequestServices.GetRequiredService<IEncryptionService>();
        var rolId = int.Parse(encryptionService.Decrypt(rolIdClaim));
        var menu = _authService.GetMenuByRole(rolId);
        return Ok(menu);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {

        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (result == null)
                return Unauthorized(new { message = "Token inválido o expirado." });

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
