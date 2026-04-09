using ViamaticaApi.Application.DTOs.Clients;

namespace ViamaticaApi.Application.Interfaces;

public interface IClientService
{
    Task<ClientResponseDto> CreateAsync(CreateClientDto dto);
    Task<IEnumerable<ClientResponseDto>> GetAllAsync();
    Task<ClientResponseDto> GetByIdAsync(int clientId);
    Task<ClientResponseDto?> GetByIdentificationAsync(string identification);
    Task<ClientResponseDto> UpdateAsync(int clientId, UpdateClientDto dto);
    Task DeleteAsync(int clientId);
    Task<ClientResponseDto> ChangeStatusAsync(int clientId, bool active);
}
