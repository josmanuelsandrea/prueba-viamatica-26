using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ViamaticaApi.Application.DTOs.Auth;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryption;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        AppDbContext context,
        IEncryptionService encryption,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _encryption = encryption;
        _configuration = configuration;
        _logger = logger;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.Active);

        if (user == null)
        {
            _logger.LogWarning("Intento de login fallido: usuario {Username} no encontrado", request.Username);
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var decryptedPassword = _encryption.Decrypt(user.Password);
        if (decryptedPassword != request.Password)
        {
            _logger.LogWarning("Intento de login fallido: contraseña incorrecta para usuario {Username}", request.Username);
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        if (user.UserstatusStatusid != "ACT")
        {
            _logger.LogWarning("Intento de login fallido: usuario {Username} con estado {Status}", request.Username, user.UserstatusStatusid);
            throw new UnauthorizedAccessException("Usuario inactivo, bloqueado o pendiente de aprobación.");
        }

        var token = GenerateJwtToken(user.Userid, user.Username, user.Rol.Rolname, user.RolRolid);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Userid);
        var expiration = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpirationHours", 8));

        _logger.LogInformation("Login exitoso para usuario {Username} con rol {Rol}", user.Username, user.Rol.Rolname);

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.Expires,
            Expiration = expiration,
            UserId = user.Userid,
            Username = user.Username,
            RolName = user.Rol.Rolname,
            RolId = user.RolRolid,
            Menu = GetMenuByRole(user.RolRolid)
        };
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Refresh token inválido.");

        int userId = await _refreshTokenService.ValidateRefreshToken(refreshToken);
        var user = await _context.Users
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Userid == userId && u.Active);

        if (user == null)
        {
            _logger.LogWarning("Refresh token: usuario {UserId} no encontrado", userId);
            throw new UnauthorizedAccessException("Usuario inválido.");
        }

        var refreshResult = await _refreshTokenService.RefreshTokenAsync(refreshToken);
        if (refreshResult == null)
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");        

        var jwt = GenerateJwtToken(user.Userid, user.Username, user.Rol.Rolname, user.RolRolid);
        var expiration = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpirationHours", 8));

        _logger.LogInformation("Refresh token exitoso para usuario {Username}", user.Username);

        return new RefreshTokenResponseDto
        {
            Token = jwt,
            RefreshToken = refreshResult.Value.RefreshToken,
            RefreshTokenExpiration = refreshResult.Value.RefreshTokenExpires
        };
    }

    public async Task<bool> RecoverPasswordAsync(RecoverPasswordRequestDto request)
    {
        var encryptedEmail = _encryption.Encrypt(request.Email);
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == encryptedEmail && u.Active);

        if (user == null)
        {
            // Respuesta generica para no revelar si el email existe
            _logger.LogInformation("Solicitud de recuperación de contraseña para email no registrado");
            return true;
        }

        _logger.LogInformation("Solicitud de recuperación de contraseña para usuario {Username}", user.Username);
        // En una implementación real se enviaría un correo con un enlace de recuperación
        return true;
    }

    public List<MenuItemDto> GetMenuByRole(int rolId)
    {
        return rolId switch
        {
            1 => // Administrador
            [
                new MenuItemDto { Label = "Bienvenida", Route = "/welcome", Icon = "pi pi-home" },
                new MenuItemDto { Label = "Dashboard", Route = "/dashboard", Icon = "pi pi-chart-bar" },
                new MenuItemDto { Label = "Usuarios", Route = "/admin/users", Icon = "pi pi-users" },
                new MenuItemDto { Label = "Cajas", Route = "/admin/cajas", Icon = "pi pi-building" },
                new MenuItemDto { Label = "Reportes", Route = "/admin/reportes", Icon = "pi pi-chart-line" },
            ],
            2 => // Gestor
            [
                new MenuItemDto { Label = "Bienvenida", Route = "/welcome", Icon = "pi pi-home" },
                new MenuItemDto { Label = "Asignación de Turnos", Route = "/gestor/turnos", Icon = "pi pi-ticket" },
                new MenuItemDto { Label = "Gestión de Cajas", Route = "/gestor/cajas", Icon = "pi pi-building" },
            ],
            3 => // Cajero
            [
                new MenuItemDto { Label = "Bienvenida", Route = "/welcome", Icon = "pi pi-home" },
                new MenuItemDto { Label = "Atenciones", Route = "/cajero/atenciones", Icon = "pi pi-ticket" },
                new MenuItemDto { Label = "Clientes", Route = "/cajero/clientes", Icon = "pi pi-user" },
                new MenuItemDto { Label = "Contratos", Route = "/cajero/contratos", Icon = "pi pi-file" },
                new MenuItemDto { Label = "Pagos", Route = "/cajero/pagos", Icon = "pi pi-credit-card" },
            ],
            _ => []
        };
    }

    private string GenerateJwtToken(int userId, string username, string rol, int rolId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddHours(_configuration.GetValue<int>("Jwt:ExpirationHours", 8));

        // Claims encriptados
        var claims = new[]
        {
            new Claim("userid", _encryption.Encrypt(userId.ToString())),
            new Claim("username", _encryption.Encrypt(username)),
            new Claim("rol", _encryption.Encrypt(rol)),
            new Claim("rolid", _encryption.Encrypt(rolId.ToString())),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
