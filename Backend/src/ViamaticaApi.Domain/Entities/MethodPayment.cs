namespace ViamaticaApi.Domain.Entities;

public class MethodPayment
{
    public int Methodpaymentid { get; set; }
    public string Description { get; set; } = string.Empty;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
