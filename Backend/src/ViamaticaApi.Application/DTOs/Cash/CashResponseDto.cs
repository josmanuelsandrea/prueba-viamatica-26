namespace ViamaticaApi.Application.DTOs.Cash;

public class CashResponseDto
{
    public int CashId { get; set; }
    public string CashDescription { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<UserCashDto> AssignedUsers { get; set; } = new();
}
