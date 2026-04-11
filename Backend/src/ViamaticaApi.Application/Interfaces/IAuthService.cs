using ViamaticaApi.Application.DTOs.Auth;

namespace ViamaticaApi.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<bool> RecoverPasswordAsync(RecoverPasswordRequestDto request);
    List<MenuItemDto> GetMenuByRole(int rolId);
    public Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken);
}