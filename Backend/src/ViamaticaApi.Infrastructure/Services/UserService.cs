using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViamaticaApi.Application.DTOs.Users;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Domain.Entities;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryption;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, IEncryptionService encryption, ILogger<UserService> logger)
    {
        _context = context;
        _encryption = encryption;
        _logger = logger;
    }

    public async Task<UserResponseDto> CreateAsync(CreateUserDto dto, int creatorUserId, int creatorRolId)
    {
        ValidateUsername(dto.Username);
        ValidatePassword(dto.Password);

        var exists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
        if (exists)
            throw new InvalidOperationException($"El nombre de usuario '{dto.Username}' ya está en uso.");

        // Solo roles 2 (Gestor) y 3 (Cajero) pueden ser creados
        if (dto.RolId != 2 && dto.RolId != 3)
            throw new InvalidOperationException("Solo se pueden crear usuarios con rol Gestor o Cajero.");

        // Admin (1) aprueba automáticamente; Gestor (2) deja en PEN
        var statusId = creatorRolId == 1 ? "ACT" : "PEN";
        int? approvalId = creatorRolId == 1 ? creatorUserId : null;
        DateTime? dateApproval = creatorRolId == 1 ? DateTime.UtcNow : null;

        var user = new User
        {
            Username = dto.Username,
            Email = _encryption.Encrypt(dto.Email),
            Password = _encryption.Encrypt(dto.Password),
            RolRolid = dto.RolId,
            UserstatusStatusid = statusId,
            Usercreate = creatorUserId,
            Userapproval = approvalId,
            Dateapproval = dateApproval,
            Creationdate = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {Username} creado por {CreatorId} con rol {RolId}. Estado: {Status}",
            user.Username, creatorUserId, dto.RolId, statusId);

        return await GetByIdAsync(user.Userid);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync(int? rolId, string? statusId)
    {
        var query = _context.Users
            .Include(u => u.Rol)
            .Include(u => u.UserStatus)
            .Include(u => u.CreatedByUser)
            .Include(u => u.ApprovedByUser)
            .AsQueryable();

        if (rolId.HasValue)
            query = query.Where(u => u.RolRolid == rolId.Value);

        if (!string.IsNullOrEmpty(statusId))
            query = query.Where(u => u.UserstatusStatusid == statusId);

        var users = await query.OrderBy(u => u.Creationdate).ToListAsync();
        return users.Select(MapToResponse);
    }

    public async Task<UserResponseDto> GetByIdAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Rol)
            .Include(u => u.UserStatus)
            .Include(u => u.CreatedByUser)
            .Include(u => u.ApprovedByUser)
            .FirstOrDefaultAsync(u => u.Userid == userId)
            ?? throw new KeyNotFoundException($"Usuario con ID {userId} no encontrado.");

        return MapToResponse(user);
    }

    public async Task<UserResponseDto> UpdateAsync(int userId, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException($"Usuario con ID {userId} no encontrado.");

        user.Email = _encryption.Encrypt(dto.Email);
        user.RolRolid = dto.RolId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UserId} actualizado", userId);

        return await GetByIdAsync(userId);
    }

    public async Task DeleteAsync(int userId, int deletedByUserId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException($"Usuario con ID {userId} no encontrado.");

        user.Active = false;
        user.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UserId} eliminado lógicamente por {DeletedBy}", userId, deletedByUserId);
    }

    public async Task<UserResponseDto> ApproveAsync(int userId, int approvedByUserId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException($"Usuario con ID {userId} no encontrado.");

        if (user.UserstatusStatusid != "PEN")
            throw new InvalidOperationException("Solo se pueden aprobar usuarios con estado Pendiente.");

        user.UserstatusStatusid = "ACT";
        user.Userapproval = approvedByUserId;
        user.Dateapproval = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UserId} aprobado por {ApprovedBy}", userId, approvedByUserId);

        return await GetByIdAsync(userId);
    }

    public async Task<UserResponseDto> ChangeStatusAsync(int userId, ChangeUserStatusDto dto)
    {
        var validStatuses = new[] { "ACT", "INA", "BLO" };
        if (!validStatuses.Contains(dto.StatusId))
            throw new InvalidOperationException($"Estado inválido. Estados permitidos: {string.Join(", ", validStatuses)}");

        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException($"Usuario con ID {userId} no encontrado.");

        user.UserstatusStatusid = dto.StatusId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Estado del usuario {UserId} cambiado a {Status}", userId, dto.StatusId);

        return await GetByIdAsync(userId);
    }

    private UserResponseDto MapToResponse(User user) => new()
    {
        UserId = user.Userid,
        Username = user.Username,
        Email = _encryption.Decrypt(user.Email),
        RolId = user.RolRolid,
        RolName = user.Rol?.Rolname ?? string.Empty,
        StatusId = user.UserstatusStatusid,
        StatusDescription = user.UserStatus?.Description ?? string.Empty,
        CreationDate = user.Creationdate,
        UserCreateId = user.Usercreate,
        UserCreateName = user.CreatedByUser?.Username,
        UserApprovalId = user.Userapproval,
        UserApprovalName = user.ApprovedByUser?.Username,
        DateApproval = user.Dateapproval,
        Active = user.Active
    };

    private static void ValidateUsername(string username)
    {
        if (username.Length < 8 || username.Length > 20)
            throw new ArgumentException("El nombre de usuario debe tener entre 8 y 20 caracteres.");

        if (!username.All(c => char.IsLetterOrDigit(c)))
            throw new ArgumentException("El nombre de usuario solo puede contener letras y números.");

        if (!username.Any(char.IsDigit))
            throw new ArgumentException("El nombre de usuario debe contener al menos un número.");
    }

    private static void ValidatePassword(string password)
    {
        if (password.Length < 8 || password.Length > 30)
            throw new ArgumentException("La contraseña debe tener entre 8 y 30 caracteres.");

        if (!password.Any(char.IsUpper))
            throw new ArgumentException("La contraseña debe contener al menos una letra mayúscula.");

        if (!password.Any(char.IsDigit))
            throw new ArgumentException("La contraseña debe contener al menos un número.");
    }
}
