namespace ViamaticaApi.Application.DTOs.Payments;

public class PaymentResponseDto
{
    public int PaymentId { get; set; }
    public int ContractId { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int AttentionId { get; set; }
    public DateTime CreatedAt { get; set; }
}
