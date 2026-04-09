namespace ViamaticaApi.Application.DTOs.Payments;

public class CreatePaymentDto
{
    public int ContractId { get; set; }
    public decimal Amount { get; set; }
    public int AttentionId { get; set; }
}
