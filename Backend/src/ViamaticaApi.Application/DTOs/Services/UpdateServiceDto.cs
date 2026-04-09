namespace ViamaticaApi.Application.DTOs.Services;

public class UpdateServiceDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public int SpeedMbps { get; set; }
    public decimal Price { get; set; }
}
