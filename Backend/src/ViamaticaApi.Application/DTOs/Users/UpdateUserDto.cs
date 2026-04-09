namespace ViamaticaApi.Application.DTOs.Users;

public class UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public int RolId { get; set; }
}
