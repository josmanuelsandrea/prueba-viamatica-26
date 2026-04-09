namespace ViamaticaApi.Application.DTOs.Devices;

public class CreateDeviceDto
{
    public string DeviceName { get; set; } = string.Empty;
    public int ServiceId { get; set; }
}
