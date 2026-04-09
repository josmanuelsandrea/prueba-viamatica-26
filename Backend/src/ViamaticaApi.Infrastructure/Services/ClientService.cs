using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViamaticaApi.Application.DTOs.Clients;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Domain.Entities;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClientService> _logger;

    public ClientService(AppDbContext context, ILogger<ClientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ClientResponseDto> CreateAsync(CreateClientDto dto)
    {
        Validate(dto.Identification, dto.Phonenumber, dto.Address, dto.Referenceaddress);

        var existing = await _context.Clients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Identification == dto.Identification);

        if (existing != null)
            throw new InvalidOperationException($"Ya existe un cliente registrado con la identificación {dto.Identification}.");

        var client = new Client
        {
            Name = dto.Name,
            Lastname = dto.Lastname,
            Identification = dto.Identification,
            Email = dto.Email,
            Phonenumber = dto.Phonenumber,
            Address = dto.Address,
            Referenceaddress = dto.Referenceaddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cliente {Identification} creado con ID {ClientId}", dto.Identification, client.Clientid);

        return MapToResponse(client);
    }

    public async Task<IEnumerable<ClientResponseDto>> GetAllAsync()
    {
        var clients = await _context.Clients
            .OrderBy(c => c.Lastname)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return clients.Select(MapToResponse);
    }

    public async Task<ClientResponseDto> GetByIdAsync(int clientId)
    {
        var client = await _context.Clients.FindAsync(clientId)
            ?? throw new KeyNotFoundException($"Cliente con ID {clientId} no encontrado.");

        return MapToResponse(client);
    }

    public async Task<ClientResponseDto?> GetByIdentificationAsync(string identification)
    {
        var client = await _context.Clients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Identification == identification);

        return client == null ? null : MapToResponse(client);
    }

    public async Task<ClientResponseDto> UpdateAsync(int clientId, UpdateClientDto dto)
    {
        ValidatePhone(dto.Phonenumber);
        ValidateAddress(dto.Address, dto.Referenceaddress);

        var client = await _context.Clients.FindAsync(clientId)
            ?? throw new KeyNotFoundException($"Cliente con ID {clientId} no encontrado.");

        client.Name = dto.Name;
        client.Lastname = dto.Lastname;
        client.Email = dto.Email;
        client.Phonenumber = dto.Phonenumber;
        client.Address = dto.Address;
        client.Referenceaddress = dto.Referenceaddress;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cliente {ClientId} actualizado", clientId);

        return MapToResponse(client);
    }

    public async Task DeleteAsync(int clientId)
    {
        var client = await _context.Clients.FindAsync(clientId)
            ?? throw new KeyNotFoundException($"Cliente con ID {clientId} no encontrado.");

        client.Active = false;
        client.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cliente {ClientId} eliminado lógicamente", clientId);
    }

    public async Task<ClientResponseDto> ChangeStatusAsync(int clientId, bool active)
    {
        var client = await _context.Clients.FindAsync(clientId)
            ?? throw new KeyNotFoundException($"Cliente con ID {clientId} no encontrado.");

        client.Active = active;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Estado del cliente {ClientId} cambiado a {Active}", clientId, active);

        return MapToResponse(client);
    }

    private static ClientResponseDto MapToResponse(Client client) => new()
    {
        ClientId = client.Clientid,
        Name = client.Name,
        Lastname = client.Lastname,
        Identification = client.Identification,
        Email = client.Email,
        Phonenumber = client.Phonenumber,
        Address = client.Address,
        Referenceaddress = client.Referenceaddress,
        Active = client.Active,
        CreatedAt = client.CreatedAt,
        UpdatedAt = client.UpdatedAt
    };

    private static void Validate(string identification, string phone, string address, string reference)
    {
        if (!Regex.IsMatch(identification, @"^\d{10,13}$"))
            throw new ArgumentException("La identificación debe tener entre 10 y 13 dígitos numéricos.");

        ValidatePhone(phone);
        ValidateAddress(address, reference);
    }

    private static void ValidatePhone(string phone)
    {
        if (!Regex.IsMatch(phone, @"^09\d{8,}$"))
            throw new ArgumentException("El teléfono debe comenzar con 09, tener mínimo 10 dígitos y solo números.");
    }

    private static void ValidateAddress(string address, string reference)
    {
        if (address.Length < 20 || address.Length > 100)
            throw new ArgumentException("La dirección debe tener entre 20 y 100 caracteres.");

        if (reference.Length < 20 || reference.Length > 100)
            throw new ArgumentException("La referencia de dirección debe tener entre 20 y 100 caracteres.");
    }
}
