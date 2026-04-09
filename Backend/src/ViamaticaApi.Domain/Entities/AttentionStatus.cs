namespace ViamaticaApi.Domain.Entities;

public class AttentionStatus
{
    public int Statusid { get; set; }
    public string Description { get; set; } = string.Empty;

    public ICollection<Attention> Attentions { get; set; } = new List<Attention>();
}
