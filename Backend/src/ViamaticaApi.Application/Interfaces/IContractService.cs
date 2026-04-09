using ViamaticaApi.Application.DTOs.Contracts;

namespace ViamaticaApi.Application.Interfaces;

public interface IContractService
{
    Task<IEnumerable<ContractResponseDto>> GetAllAsync(int? clientId, string? statusCode);
    Task<ContractResponseDto> GetByIdAsync(int contractId);
    Task<ContractResponseDto> CreateAsync(CreateContractDto dto);
    Task<ContractResponseDto> ChangeServiceAsync(int contractId, ChangeServiceDto dto);
    Task<ContractResponseDto> ChangePaymentMethodAsync(int contractId, ChangePaymentMethodDto dto);
    Task<ContractResponseDto> CancelAsync(int contractId);
}
