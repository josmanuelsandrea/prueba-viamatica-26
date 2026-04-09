namespace ViamaticaApi.Domain.Entities;

public class Service
{
    public int Serviceid { get; set; }
    public string Servicename { get; set; } = string.Empty;
    public string Servicedescription { get; set; } = string.Empty;
    public int SpeedMbps { get; set; }
    public decimal Price { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
