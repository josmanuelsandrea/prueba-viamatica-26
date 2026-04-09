namespace ViamaticaApi.Domain.Entities;

public class StatusContract
{
    public string Statusid { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
