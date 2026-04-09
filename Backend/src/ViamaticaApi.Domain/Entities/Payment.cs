namespace ViamaticaApi.Domain.Entities;

public class Payment
{
    public int Paymentid { get; set; }
    public DateTime Paydate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public int ClientClientid { get; set; }
    public int ContractContractid { get; set; }
    public int MethodpaymentMethodpaymentid { get; set; }
    public int? AttentionAttentionid { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Client Client { get; set; } = null!;
    public Contract Contract { get; set; } = null!;
    public MethodPayment MethodPayment { get; set; } = null!;
    public Attention? Attention { get; set; }
}
