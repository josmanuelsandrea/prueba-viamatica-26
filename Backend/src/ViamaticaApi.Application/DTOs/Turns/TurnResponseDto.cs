namespace ViamaticaApi.Application.DTOs.Turns;

public class TurnResponseDto
{
    public int TurnId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CashId { get; set; }
    public string CashDescription { get; set; } = string.Empty;
    public int UserGestorId { get; set; }
    public string UserGestorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
