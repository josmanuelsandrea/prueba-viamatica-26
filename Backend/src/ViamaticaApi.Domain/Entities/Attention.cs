namespace ViamaticaApi.Domain.Entities;

public class Attention
{
    public int Attentionid { get; set; }
    public int TurnTurnid { get; set; }
    public int? ClientClientid { get; set; }
    public string AttentiontypeAttentiontypeid { get; set; } = string.Empty;
    public int AttentionstatusStatusid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Turn Turn { get; set; } = null!;
    public Client? Client { get; set; }
    public AttentionType AttentionType { get; set; } = null!;
    public AttentionStatus AttentionStatus { get; set; } = null!;
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
