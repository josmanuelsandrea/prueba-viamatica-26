namespace ViamaticaApi.Domain.Entities;

public class Contract
{
    public int Contractid { get; set; }
    public DateTime Startdate { get; set; }
    public DateTime? Enddate { get; set; }
    public int ServiceServiceid { get; set; }
    public string StatuscontractStatusid { get; set; } = string.Empty;
    public int ClientClientid { get; set; }
    public int MethodpaymentMethodpaymentid { get; set; }
    public int? AttentionAttentionid { get; set; }
    public int? ParentContractid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Service Service { get; set; } = null!;
    public StatusContract StatusContract { get; set; } = null!;
    public Client Client { get; set; } = null!;
    public MethodPayment MethodPayment { get; set; } = null!;
    public Attention? Attention { get; set; }
    public Contract? ParentContract { get; set; }
    public ICollection<Contract> ChildContracts { get; set; } = new List<Contract>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
