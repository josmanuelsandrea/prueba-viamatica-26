namespace ViamaticaApi.Domain.Entities;

public class Device
{
    public int Deviceid { get; set; }
    public string Devicename { get; set; } = string.Empty;
    public int ServiceServiceid { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Service Service { get; set; } = null!;
}
