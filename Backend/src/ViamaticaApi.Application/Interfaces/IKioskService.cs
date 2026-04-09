using ViamaticaApi.Application.DTOs.Attentions;
using ViamaticaApi.Application.DTOs.Kiosk;

namespace ViamaticaApi.Application.Interfaces;

public interface IKioskService
{
    Task<KioskClientResponseDto?> FindClientByIdentificationAsync(string identification);
    Task<KioskClientResponseDto> RegisterClientAsync(KioskRegisterClientDto dto);
    Task<KioskTurnResponseDto> RequestTurnAsync(KioskRequestTurnDto dto);
}
