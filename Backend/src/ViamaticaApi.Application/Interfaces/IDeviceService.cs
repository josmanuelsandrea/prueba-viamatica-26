using ViamaticaApi.Application.DTOs.Devices;

namespace ViamaticaApi.Application.Interfaces;

public interface IDeviceService
{
    Task<IEnumerable<DeviceResponseDto>> GetAllAsync(int? serviceId = null);
    Task<DeviceResponseDto> GetByIdAsync(int deviceId);
    Task<DeviceResponseDto> CreateAsync(CreateDeviceDto dto);
    Task<DeviceResponseDto> UpdateAsync(int deviceId, UpdateDeviceDto dto);
    Task DeleteAsync(int deviceId);
}
