namespace ViamaticaApi.Application.DTOs.Attentions;

public class AttentionResponseDto
{
    public int AttentionId { get; set; }
    public int TurnId { get; set; }
    public string TurnDescription { get; set; } = string.Empty;
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientIdentification { get; set; }
    public string AttentionTypeId { get; set; } = string.Empty;
    public string AttentionTypeDescription { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
