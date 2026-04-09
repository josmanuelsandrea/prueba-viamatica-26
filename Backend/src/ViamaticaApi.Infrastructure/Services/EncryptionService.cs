using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using ViamaticaApi.Application.Interfaces;

namespace ViamaticaApi.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key no está configurada.");

        // AES-256 requiere 32 bytes de clave
        _key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
        // IV fijo de 16 bytes derivado de la clave para consistencia
        _iv = Encoding.UTF8.GetBytes(keyString.PadRight(16).Substring(0, 16));
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
