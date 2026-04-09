using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViamaticaApi.Application.DTOs.Devices;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Domain.Entities;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class DeviceService : IDeviceService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(AppDbContext context, ILogger<DeviceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DeviceResponseDto>> GetAllAsync(int? serviceId = null)
    {
        var query = _context.Devices
            .Include(d => d.Service)
            .AsQueryable();

        if (serviceId.HasValue)
            query = query.Where(d => d.ServiceServiceid == serviceId.Value);

        var devices = await query.OrderBy(d => d.Devicename).ToListAsync();
        return devices.Select(MapToResponse);
    }

    public async Task<DeviceResponseDto> GetByIdAsync(int deviceId)
    {
        var device = await _context.Devices
            .Include(d => d.Service)
            .FirstOrDefaultAsync(d => d.Deviceid == deviceId)
            ?? throw new KeyNotFoundException($"Dispositivo con ID {deviceId} no encontrado.");

        return MapToResponse(device);
    }

    public async Task<DeviceResponseDto> CreateAsync(CreateDeviceDto dto)
    {
        var service = await _context.Services.FindAsync(dto.ServiceId)
            ?? throw new KeyNotFoundException($"Servicio con ID {dto.ServiceId} no encontrado.");

        var device = new Device
        {
            Devicename = dto.DeviceName,
            ServiceServiceid = dto.ServiceId,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        // Reload with navigation property for mapping
        device.Service = service;

        _logger.LogInformation("Dispositivo {DeviceName} creado con ID {DeviceId} para servicio {ServiceId}",
            device.Devicename, device.Deviceid, device.ServiceServiceid);

        return MapToResponse(device);
    }

    public async Task<DeviceResponseDto> UpdateAsync(int deviceId, UpdateDeviceDto dto)
    {
        var device = await _context.Devices
            .Include(d => d.Service)
            .FirstOrDefaultAsync(d => d.Deviceid == deviceId)
            ?? throw new KeyNotFoundException($"Dispositivo con ID {deviceId} no encontrado.");

        device.Devicename = dto.DeviceName;
        device.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Dispositivo {DeviceId} actualizado", deviceId);

        return MapToResponse(device);
    }

    public async Task DeleteAsync(int deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId)
            ?? throw new KeyNotFoundException($"Dispositivo con ID {deviceId} no encontrado.");

        device.Active = false;
        device.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Dispositivo {DeviceId} eliminado (logical delete)", deviceId);
    }

    private static DeviceResponseDto MapToResponse(Device d) => new()
    {
        DeviceId = d.Deviceid,
        DeviceName = d.Devicename,
        ServiceId = d.ServiceServiceid,
        ServiceName = d.Service?.Servicename ?? string.Empty,
        Active = d.Active,
        CreatedAt = d.CreatedAt
    };
}
