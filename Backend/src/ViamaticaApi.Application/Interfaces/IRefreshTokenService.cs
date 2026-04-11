using ViamaticaApi.Application.DTOs.Auth;

namespace ViamaticaApi.Application.Interfaces
{
    public interface IRefreshTokenService
    {
        public Task<(string Token, DateTime Expires)> GenerateRefreshTokenAsync(int userId);
        public Task<bool> RevokeRefreshTokenAsync(string token);
        public Task<int> ValidateRefreshToken(string token);
        public Task<(string RefreshToken, DateTime RefreshTokenExpires)?> RefreshTokenAsync(string token);
    }
}