namespace ViamaticaApi.Application.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RolName { get; set; } = string.Empty;
    public int RolId { get; set; }
    public List<MenuItemDto> Menu { get; set; } = [];
}
