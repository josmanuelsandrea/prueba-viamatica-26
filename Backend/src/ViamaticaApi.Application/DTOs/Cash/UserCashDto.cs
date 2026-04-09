namespace ViamaticaApi.Application.DTOs.Cash;

public class UserCashDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
}
