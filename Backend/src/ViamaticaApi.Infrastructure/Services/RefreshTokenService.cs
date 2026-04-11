using System.Security.Cryptography;
using System.Text.Json;
using StackExchange.Redis;
using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Infrastructure.Services;
public class RefreshTokenService : IRefreshTokenService
{
    private const string KeyPrefix = "auth:refresh:";
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDatabase _redis;

    public RefreshTokenService(IConnectionMultiplexer connection)
    {
        _redis = connection.GetDatabase();
    }

    private static RedisKey Key(string token) => KeyPrefix + token;

    public async Task<(string Token, DateTime Expires)> GenerateRefreshTokenAsync(int userId)
    {
        var token = CreateOpaqueToken();
        var expires = DateTime.UtcNow.Add(Lifetime);

        var json = JsonSerializer.Serialize(
            new StoredRefreshToken(userId, expires, Revoked: false),
            Json);

        await _redis.StringSetAsync(Key(token), json, Lifetime);

        return (token, expires);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        // Borrar el token equivale a revocarlo: ya no será válido.
        return await _redis.KeyDeleteAsync(Key(token));
    }

    public async Task<int> ValidateRefreshToken(string token)
    {
        var raw = await _redis.StringGetAsync(Key(token));
        if (raw.IsNullOrEmpty)
            return 0;

        StoredRefreshToken? data;
        try
        {
            data = JsonSerializer.Deserialize<StoredRefreshToken>((string)raw!, Json);
        }
        catch (JsonException)
        {
            return 0;
        }

        if (data is null || data.Revoked || data.ExpiresUtc <= DateTime.UtcNow)
            return 0;

        return data.UserId;
    }

    public async Task<(string RefreshToken, DateTime RefreshTokenExpires)?> RefreshTokenAsync(string token)
    {
        var userId = await ValidateRefreshToken(token);
        if (userId == 0)
            return null;

        await RevokeRefreshTokenAsync(token);
        return await GenerateRefreshTokenAsync(userId);
    }

    private static string CreateOpaqueToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private sealed record StoredRefreshToken(int UserId, DateTime ExpiresUtc, bool Revoked);
}
