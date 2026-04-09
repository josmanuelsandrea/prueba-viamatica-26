namespace ViamaticaApi.Application.DTOs.Contracts;

public class CreateContractDto
{
    public int ClientId { get; set; }
    public int ServiceId { get; set; }
    public int MethodPaymentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
