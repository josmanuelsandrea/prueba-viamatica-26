using ViamaticaApi.Application.DTOs.Devices;

namespace ViamaticaApi.Application.DTOs.Services;

public class ServiceResponseDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public int SpeedMbps { get; set; }
    public decimal Price { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DeviceResponseDto> Devices { get; set; } = new();
}
