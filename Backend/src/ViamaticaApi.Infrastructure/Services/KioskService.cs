using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViamaticaApi.Application.DTOs.Kiosk;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Domain.Entities;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class KioskService : IKioskService
{
    private readonly AppDbContext _context;
    private readonly ILogger<KioskService> _logger;

    public KioskService(AppDbContext context, ILogger<KioskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<KioskClientResponseDto?> FindClientByIdentificationAsync(string identification)
    {
        var client = await _context.Clients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Identification == identification && c.Active);

        if (client == null) return null;

        return new KioskClientResponseDto
        {
            ClientId = client.Clientid,
            Name = client.Name,
            Lastname = client.Lastname,
            Identification = client.Identification,
            IsNew = false
        };
    }

    public async Task<KioskClientResponseDto> RegisterClientAsync(KioskRegisterClientDto dto)
    {
        // Validate identification is unique
        var exists = await _context.Clients
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Identification == dto.Identification);

        if (exists)
            throw new InvalidOperationException($"Ya existe un cliente con identificación {dto.Identification}.");

        // Validate identification format
        if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Identification, @"^\d{10,13}$"))
            throw new ArgumentException("La identificación debe tener entre 10 y 13 dígitos numéricos.");

        // Validate phone format
        if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Phone, @"^09\d{8,}$"))
            throw new ArgumentException("El teléfono debe iniciar con 09 y tener mínimo 10 dígitos.");

        var client = new Client
        {
            Name = dto.Name,
            Lastname = dto.Lastname,
            Identification = dto.Identification,
            Phonenumber = dto.Phone,
            Address = dto.Address,
            Referenceaddress = dto.ReferenceAddress,
            Email = dto.Email,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cliente {ClientId} registrado desde kiosco con identificación {Identification}", client.Clientid, dto.Identification);

        return new KioskClientResponseDto
        {
            ClientId = client.Clientid,
            Name = client.Name,
            Lastname = client.Lastname,
            Identification = client.Identification,
            IsNew = true
        };
    }

    public async Task<KioskTurnResponseDto> RequestTurnAsync(KioskRequestTurnDto dto)
    {
        // Validate client exists
        var client = await _context.Clients.FindAsync(dto.ClientId)
            ?? throw new KeyNotFoundException($"Cliente con ID {dto.ClientId} no encontrado.");

        // Validate attention type exists
        var attentionType = await _context.AttentionTypes.FindAsync(dto.AttentionTypeId)
            ?? throw new KeyNotFoundException($"Tipo de atención '{dto.AttentionTypeId}' no encontrado.");

        // Auto-assign: find the active cash with fewer pending turns today
        var today = DateTime.UtcNow.Date;

        var activeCash = await _context.Cashes
            .Where(c => c.Active)
            .OrderBy(c => _context.Turns.Count(t => t.CashCashid == c.Cashid && t.Date.Date == today))
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No hay cajas activas disponibles en este momento.");

        // Find any active gestor to assign the turn
        var gestor = await _context.Users
            .Where(u => u.RolRolid == 2 && u.UserstatusStatusid == "ACT")
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No hay gestores disponibles en este momento.");

        // Generate turn description via stored procedure
        var description = await GenerateTurnDescriptionAsync(activeCash.Cashid, dto.AttentionTypeId);

        var turn = new Turn
        {
            Description = description,
            Date = DateTime.UtcNow,
            CashCashid = activeCash.Cashid,
            Usergestorid = gestor.Userid,
            CreatedAt = DateTime.UtcNow
        };

        _context.Turns.Add(turn);
        await _context.SaveChangesAsync();

        // Crear la atención vinculada al turno con estado Pendiente
        var attention = new Attention
        {
            TurnTurnid = turn.Turnid,
            ClientClientid = dto.ClientId,
            AttentiontypeAttentiontypeid = dto.AttentionTypeId,
            AttentionstatusStatusid = 1, // Pendiente
            CreatedAt = DateTime.UtcNow
        };
        _context.Attentions.Add(attention);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Turno {Description} generado desde kiosco para cliente {ClientId} en caja {CashId}", description, dto.ClientId, activeCash.Cashid);

        return new KioskTurnResponseDto
        {
            TurnId = turn.Turnid,
            TurnNumber = description,
            AttentionTypeDescription = attentionType.Description,
            CashDescription = activeCash.Cashdescription,
            CreatedAt = turn.CreatedAt
        };
    }

    private async Task<string> GenerateTurnDescriptionAsync(int cashId, string attentionTypeId)
    {
        var connection = _context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;

        if (!wasOpen) await connection.OpenAsync();

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
            if (!wasOpen) await connection.CloseAsync();
        }
    }
}
