using ViamaticaApi.Application.DTOs.Turns;

namespace ViamaticaApi.Application.Interfaces;

public interface ITurnService
{
    Task<TurnResponseDto> CreateAsync(CreateTurnDto dto, int gestorId);
    Task<IEnumerable<TurnResponseDto>> GetAllAsync(int? cashId, DateTime? date);
    Task<TurnResponseDto> GetByIdAsync(int turnId);
}
