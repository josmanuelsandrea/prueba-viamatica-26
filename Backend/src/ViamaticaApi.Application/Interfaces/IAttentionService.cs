using ViamaticaApi.Application.DTOs.Attentions;

namespace ViamaticaApi.Application.Interfaces;

public interface IAttentionService
{
    Task<AttentionResponseDto> CreateAsync(CreateAttentionDto dto);
    Task<AttentionResponseDto> UpdateStatusAsync(int attentionId, UpdateAttentionStatusDto dto);
    Task<IEnumerable<AttentionResponseDto>> GetAllAsync(int? cashId, DateTime? date);
    Task<DailySummaryDto> GetDailySummaryAsync(int userId, int rolId);
}
