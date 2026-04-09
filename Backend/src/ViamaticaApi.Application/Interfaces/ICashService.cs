using ViamaticaApi.Application.DTOs.Cash;

namespace ViamaticaApi.Application.Interfaces;

public interface ICashService
{
    Task<CashResponseDto> CreateAsync(CreateCashDto dto);
    Task<IEnumerable<CashResponseDto>> GetAllAsync();
    Task<CashResponseDto> GetByIdAsync(int cashId);
    Task<CashResponseDto> UpdateAsync(int cashId, UpdateCashDto dto);
    Task DeleteAsync(int cashId);
    Task<CashResponseDto> AssignUserAsync(int cashId, AssignUserCashDto dto);
    Task RemoveUserAsync(int cashId, int userId);
    Task<CashResponseDto> ActivateUserAsync(int cashId, int userId);
    Task<CashResponseDto> DeactivateUserAsync(int cashId, int userId);
}
