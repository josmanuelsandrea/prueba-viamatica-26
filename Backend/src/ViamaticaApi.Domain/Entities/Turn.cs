namespace ViamaticaApi.Domain.Entities;

public class Turn
{
    public int Turnid { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int CashCashid { get; set; }
    public int Usergestorid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cash Cash { get; set; } = null!;
    public User Gestor { get; set; } = null!;
    public ICollection<Attention> Attentions { get; set; } = new List<Attention>();
}
