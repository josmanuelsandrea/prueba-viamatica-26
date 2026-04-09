namespace ViamaticaApi.Domain.Entities;

public class AttentionType
{
    public string Attentiontypeid { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;

    public ICollection<Attention> Attentions { get; set; } = new List<Attention>();
}
