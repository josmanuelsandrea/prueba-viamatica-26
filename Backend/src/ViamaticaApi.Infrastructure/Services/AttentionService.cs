using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViamaticaApi.Application.DTOs.Attentions;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Domain.Entities;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class AttentionService : IAttentionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AttentionService> _logger;

    public AttentionService(AppDbContext context, ILogger<AttentionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AttentionResponseDto> CreateAsync(CreateAttentionDto dto)
    {
        var turn = await _context.Turns.FindAsync(dto.TurnId)
            ?? throw new KeyNotFoundException($"Turno con ID {dto.TurnId} no encontrado.");

        var attentionType = await _context.AttentionTypes.FindAsync(dto.AttentionTypeId)
            ?? throw new KeyNotFoundException($"Tipo de atención '{dto.AttentionTypeId}' no encontrado.");

        var attention = new Attention
        {
            TurnTurnid = dto.TurnId,
            ClientClientid = dto.ClientId,
            AttentiontypeAttentiontypeid = dto.AttentionTypeId,
            AttentionstatusStatusid = 1, // Pendiente
            CreatedAt = DateTime.UtcNow
        };

        _context.Attentions.Add(attention);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Atención {AttentionId} creada para turno {TurnId}", attention.Attentionid, dto.TurnId);

        return await GetByIdAsync(attention.Attentionid);
    }

    public async Task<AttentionResponseDto> UpdateStatusAsync(int attentionId, UpdateAttentionStatusDto dto)
    {
        var validStatuses = new[] { 1, 2, 3 };
        if (!validStatuses.Contains(dto.StatusId))
            throw new ArgumentException("Estado inválido. Valores permitidos: 1 (Pendiente), 2 (Atendido), 3 (Cancelado).");

        var attention = await _context.Attentions.FindAsync(attentionId)
            ?? throw new KeyNotFoundException($"Atención con ID {attentionId} no encontrada.");

        attention.AttentionstatusStatusid = dto.StatusId;
        attention.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Estado de atención {AttentionId} actualizado a {StatusId}", attentionId, dto.StatusId);

        return await GetByIdAsync(attentionId);
    }

    public async Task<IEnumerable<AttentionResponseDto>> GetAllAsync(int? cashId, DateTime? date)
    {
        var query = _context.Attentions
            .Include(a => a.Turn).ThenInclude(t => t.Cash)
            .Include(a => a.Client)
            .Include(a => a.AttentionType)
            .Include(a => a.AttentionStatus)
            .AsQueryable();

        if (cashId.HasValue)
            query = query.Where(a => a.Turn.CashCashid == cashId.Value);

        if (date.HasValue)
            query = query.Where(a => a.CreatedAt.Date == date.Value.Date);

        var attentions = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return attentions.Select(MapToResponse);
    }

    public async Task<DailySummaryDto> GetDailySummaryAsync(int userId, int rolId)
    {
        var today = DateTime.UtcNow.Date;
        int total;

        if (rolId == 1) // Administrador: total global
        {
            total = await _context.Attentions
                .Where(a => a.AttentionstatusStatusid == 2 && a.CreatedAt.Date == today)
                .CountAsync();
        }
        else if (rolId == 2) // Gestor: turnos que él asignó
        {
            total = await _context.Attentions
                .Include(a => a.Turn)
                .Where(a => a.AttentionstatusStatusid == 2
                         && a.CreatedAt.Date == today
                         && a.Turn.Usergestorid == userId)
                .CountAsync();
        }
        else // Cajero: sus atenciones del día (turnos de su caja activa)
        {
            var activeCashId = await _context.UserCashes
                .Where(uc => uc.UserUserid == userId && uc.IsActive)
                .Select(uc => uc.CashCashid)
                .FirstOrDefaultAsync();

            total = await _context.Attentions
                .Include(a => a.Turn)
                .Where(a => a.AttentionstatusStatusid == 2
                         && a.CreatedAt.Date == today
                         && a.Turn.CashCashid == activeCashId)
                .CountAsync();
        }

        return new DailySummaryDto { Total = total, Date = DateTime.UtcNow };
    }

    private async Task<AttentionResponseDto> GetByIdAsync(int attentionId)
    {
        var attention = await _context.Attentions
            .Include(a => a.Turn)
            .Include(a => a.Client)
            .Include(a => a.AttentionType)
            .Include(a => a.AttentionStatus)
            .FirstOrDefaultAsync(a => a.Attentionid == attentionId)
            ?? throw new KeyNotFoundException($"Atención con ID {attentionId} no encontrada.");

        return MapToResponse(attention);
    }

    private static AttentionResponseDto MapToResponse(Attention a) => new()
    {
        AttentionId = a.Attentionid,
        TurnId = a.TurnTurnid,
        TurnDescription = a.Turn?.Description ?? string.Empty,
        ClientId = a.ClientClientid,
        ClientName = a.Client != null ? $"{a.Client.Name} {a.Client.Lastname}" : null,
        ClientIdentification = a.Client?.Identification,
        AttentionTypeId = a.AttentiontypeAttentiontypeid,
        AttentionTypeDescription = a.AttentionType?.Description ?? string.Empty,
        StatusId = a.AttentionstatusStatusid,
        StatusDescription = a.AttentionStatus?.Description ?? string.Empty,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
