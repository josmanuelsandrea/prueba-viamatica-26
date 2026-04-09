namespace ViamaticaApi.Application.DTOs.Attentions;

public class CreateAttentionDto
{
    public int TurnId { get; set; }
    public int? ClientId { get; set; }
    public string AttentionTypeId { get; set; } = string.Empty;
}
