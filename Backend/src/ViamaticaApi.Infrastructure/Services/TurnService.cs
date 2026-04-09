using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using ViamaticaApi.Application.DTOs.Turns;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Domain.Entities;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class TurnService : ITurnService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TurnService> _logger;

    public TurnService(AppDbContext context, ILogger<TurnService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TurnResponseDto> CreateAsync(CreateTurnDto dto, int gestorId)
    {
        var cash = await _context.Cashes.FindAsync(dto.CashId)
            ?? throw new KeyNotFoundException($"Caja con ID {dto.CashId} no encontrada.");

        var attentionType = await _context.AttentionTypes.FindAsync(dto.AttentionTypeId)
            ?? throw new KeyNotFoundException($"Tipo de atención '{dto.AttentionTypeId}' no encontrado.");

        // Llamar al stored procedure via Dapper
        var description = await GenerateTurnDescriptionAsync(dto.CashId, dto.AttentionTypeId);

        var turn = new Turn
        {
            Description = description,
            Date = DateTime.UtcNow,
            CashCashid = dto.CashId,
            Usergestorid = gestorId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Turns.Add(turn);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Turno {Description} creado en caja {CashId} por gestor {GestorId}",
            description, dto.CashId, gestorId);

        return await GetByIdAsync(turn.Turnid);
    }

    public async Task<IEnumerable<TurnResponseDto>> GetAllAsync(int? cashId, DateTime? date)
    {
        var query = _context.Turns
            .Include(t => t.Cash)
            .Include(t => t.Gestor)
            .AsQueryable();

        if (cashId.HasValue)
            query = query.Where(t => t.CashCashid == cashId.Value);

        if (date.HasValue)
            query = query.Where(t => t.Date.Date == date.Value.Date);

        var turns = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return turns.Select(MapToResponse);
    }

    public async Task<TurnResponseDto> GetByIdAsync(int turnId)
    {
        var turn = await _context.Turns
            .Include(t => t.Cash)
            .Include(t => t.Gestor)
            .FirstOrDefaultAsync(t => t.Turnid == turnId)
            ?? throw new KeyNotFoundException($"Turno con ID {turnId} no encontrado.");

        return MapToResponse(turn);
    }

    private async Task<string> GenerateTurnDescriptionAsync(int cashId, string attentionTypeId)
    {
        var connection = _context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;

        if (!wasOpen)
            await connection.OpenAsync();

        try
        {
            var description = await connection.ExecuteScalarAsync<string>(
                "SELECT sp_generate_turn_description(@cashId, @attentionTypeId)",
                new { cashId, attentionTypeId }
            );

            return description ?? throw new InvalidOperationException("El stored procedure no retornó una descripción.");
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    private static TurnResponseDto MapToResponse(Turn turn) => new()
    {
        TurnId = turn.Turnid,
        Description = turn.Description,
        Date = turn.Date,
        CashId = turn.CashCashid,
        CashDescription = turn.Cash?.Cashdescription ?? string.Empty,
        UserGestorId = turn.Usergestorid,
        UserGestorName = turn.Gestor?.Username ?? string.Empty,
        CreatedAt = turn.CreatedAt
    };
}
