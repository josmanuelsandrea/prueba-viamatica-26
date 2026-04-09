using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViamaticaApi.Application.DTOs.Cash;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Domain.Entities;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class CashService : ICashService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CashService> _logger;

    public CashService(AppDbContext context, ILogger<CashService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CashResponseDto> CreateAsync(CreateCashDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CashDescription))
            throw new ArgumentException("La descripción de la caja es requerida.");

        var cash = new Cash
        {
            Cashdescription = dto.CashDescription,
            CreatedAt = DateTime.UtcNow
        };

        _context.Cashes.Add(cash);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Caja {CashId} creada: {Description}", cash.Cashid, cash.Cashdescription);

        return await GetByIdAsync(cash.Cashid);
    }

    public async Task<IEnumerable<CashResponseDto>> GetAllAsync()
    {
        var cashes = await _context.Cashes
            .Include(c => c.UserCashes)
                .ThenInclude(uc => uc.User)
            .OrderBy(c => c.Cashid)
            .ToListAsync();

        return cashes.Select(MapToResponse);
    }

    public async Task<CashResponseDto> GetByIdAsync(int cashId)
    {
        var cash = await _context.Cashes
            .Include(c => c.UserCashes)
                .ThenInclude(uc => uc.User)
            .FirstOrDefaultAsync(c => c.Cashid == cashId)
            ?? throw new KeyNotFoundException($"Caja con ID {cashId} no encontrada.");

        return MapToResponse(cash);
    }

    public async Task<CashResponseDto> UpdateAsync(int cashId, UpdateCashDto dto)
    {
        var cash = await _context.Cashes.FindAsync(cashId)
            ?? throw new KeyNotFoundException($"Caja con ID {cashId} no encontrada.");

        cash.Cashdescription = dto.CashDescription;
        cash.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Caja {CashId} actualizada", cashId);

        return await GetByIdAsync(cashId);
    }

    public async Task DeleteAsync(int cashId)
    {
        var cash = await _context.Cashes
            .Include(c => c.UserCashes)
            .FirstOrDefaultAsync(c => c.Cashid == cashId)
            ?? throw new KeyNotFoundException($"Caja con ID {cashId} no encontrada.");

        if (cash.UserCashes.Any(uc => uc.IsActive))
            throw new InvalidOperationException("No se puede eliminar una caja con cajeros activos.");

        cash.Active = false;
        cash.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Caja {CashId} eliminada lógicamente", cashId);
    }

    public async Task<CashResponseDto> AssignUserAsync(int cashId, AssignUserCashDto dto)
    {
        var cash = await _context.Cashes
            .Include(c => c.UserCashes)
            .FirstOrDefaultAsync(c => c.Cashid == cashId)
            ?? throw new KeyNotFoundException($"Caja con ID {cashId} no encontrada.");

        if (cash.UserCashes.Count >= 2)
            throw new InvalidOperationException("La caja ya tiene el máximo de 2 usuarios asignados.");

        var user = await _context.Users.FindAsync(dto.UserId)
            ?? throw new KeyNotFoundException($"Usuario con ID {dto.UserId} no encontrado.");

        if (user.RolRolid != 3)
            throw new InvalidOperationException("Solo se pueden asignar usuarios con rol Cajero a una caja.");

        var alreadyAssigned = cash.UserCashes.Any(uc => uc.UserUserid == dto.UserId);
        if (alreadyAssigned)
            throw new InvalidOperationException("El usuario ya está asignado a esta caja.");

        var userCash = new UserCash
        {
            UserUserid = dto.UserId,
            CashCashid = cashId,
            IsActive = false,
            AssignedAt = DateTime.UtcNow
        };

        _context.UserCashes.Add(userCash);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cajero {UserId} asignado a caja {CashId}", dto.UserId, cashId);

        return await GetByIdAsync(cashId);
    }

    public async Task RemoveUserAsync(int cashId, int userId)
    {
        var userCash = await _context.UserCashes
            .FirstOrDefaultAsync(uc => uc.CashCashid == cashId && uc.UserUserid == userId)
            ?? throw new KeyNotFoundException($"El usuario {userId} no está asignado a la caja {cashId}.");

        if (userCash.IsActive)
            throw new InvalidOperationException("No se puede quitar un cajero que está activo en la caja. Desactívelo primero.");

        _context.UserCashes.Remove(userCash);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cajero {UserId} quitado de caja {CashId}", userId, cashId);
    }

    public async Task<CashResponseDto> ActivateUserAsync(int cashId, int userId)
    {
        var hasActiveUser = await _context.UserCashes
            .AnyAsync(uc => uc.CashCashid == cashId && uc.IsActive);

        if (hasActiveUser)
            throw new InvalidOperationException("Ya existe un cajero activo en esta caja. Desactívelo primero.");

        var userCash = await _context.UserCashes
            .FirstOrDefaultAsync(uc => uc.CashCashid == cashId && uc.UserUserid == userId)
            ?? throw new KeyNotFoundException($"El usuario {userId} no está asignado a la caja {cashId}.");

        userCash.IsActive = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cajero {UserId} activado en caja {CashId}", userId, cashId);

        return await GetByIdAsync(cashId);
    }

    public async Task<CashResponseDto> DeactivateUserAsync(int cashId, int userId)
    {
        var userCash = await _context.UserCashes
            .FirstOrDefaultAsync(uc => uc.CashCashid == cashId && uc.UserUserid == userId)
            ?? throw new KeyNotFoundException($"El usuario {userId} no está asignado a la caja {cashId}.");

        userCash.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cajero {UserId} desactivado en caja {CashId}", userId, cashId);

        return await GetByIdAsync(cashId);
    }

    private static CashResponseDto MapToResponse(Cash cash) => new()
    {
        CashId = cash.Cashid,
        CashDescription = cash.Cashdescription,
        Active = cash.Active,
        CreatedAt = cash.CreatedAt,
        AssignedUsers = cash.UserCashes.Select(uc => new UserCashDto
        {
            UserId = uc.UserUserid,
            Username = uc.User?.Username ?? string.Empty,
            IsActive = uc.IsActive,
            AssignedAt = uc.AssignedAt
        }).ToList()
    };
}
