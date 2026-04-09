namespace ViamaticaApi.Domain.Entities;

public class UserCash
{
    public int UserUserid { get; set; }
    public int CashCashid { get; set; }
    public bool IsActive { get; set; } = false;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Cash Cash { get; set; } = null!;
}
