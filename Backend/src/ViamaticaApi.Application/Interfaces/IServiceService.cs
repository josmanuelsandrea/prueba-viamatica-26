using ViamaticaApi.Application.DTOs.Services;

namespace ViamaticaApi.Application.Interfaces;

public interface IServiceService
{
    Task<IEnumerable<ServiceResponseDto>> GetAllAsync(bool includeInactive = false);
    Task<ServiceResponseDto> GetByIdAsync(int serviceId);
    Task<ServiceResponseDto> CreateAsync(CreateServiceDto dto);
    Task<ServiceResponseDto> UpdateAsync(int serviceId, UpdateServiceDto dto);
    Task DeleteAsync(int serviceId);
}
