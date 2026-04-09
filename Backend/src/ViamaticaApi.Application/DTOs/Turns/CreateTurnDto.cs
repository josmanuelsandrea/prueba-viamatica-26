namespace ViamaticaApi.Application.DTOs.Turns;

public class CreateTurnDto
{
    public int CashId { get; set; }
    public string AttentionTypeId { get; set; } = string.Empty;
}
