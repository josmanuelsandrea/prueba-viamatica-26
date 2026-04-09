using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViamaticaApi.Application.DTOs.Devices;
using ViamaticaApi.Application.DTOs.Services;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Domain.Entities;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class ServiceService : IServiceService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ServiceService> _logger;

    public ServiceService(AppDbContext context, ILogger<ServiceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ServiceResponseDto>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Services
            .Include(s => s.Devices)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(s => s.Active);

        var services = await query.OrderBy(s => s.Servicename).ToListAsync();
        return services.Select(MapToResponse);
    }

    public async Task<ServiceResponseDto> GetByIdAsync(int serviceId)
    {
        var service = await _context.Services
            .Include(s => s.Devices)
            .FirstOrDefaultAsync(s => s.Serviceid == serviceId)
            ?? throw new KeyNotFoundException($"Servicio con ID {serviceId} no encontrado.");

        return MapToResponse(service);
    }

    public async Task<ServiceResponseDto> CreateAsync(CreateServiceDto dto)
    {
        var service = new Service
        {
            Servicename = dto.ServiceName,
            Servicedescription = dto.ServiceDescription,
            SpeedMbps = dto.SpeedMbps,
            Price = dto.Price,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Servicio {ServiceName} creado con ID {ServiceId}", service.Servicename, service.Serviceid);

        // Reload with devices for consistent response shape
        return await GetByIdAsync(service.Serviceid);
    }

    public async Task<ServiceResponseDto> UpdateAsync(int serviceId, UpdateServiceDto dto)
    {
        var service = await _context.Services
            .Include(s => s.Devices)
            .FirstOrDefaultAsync(s => s.Serviceid == serviceId)
            ?? throw new KeyNotFoundException($"Servicio con ID {serviceId} no encontrado.");

        service.Servicename = dto.ServiceName;
        service.Servicedescription = dto.ServiceDescription;
        service.SpeedMbps = dto.SpeedMbps;
        service.Price = dto.Price;
        service.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Servicio {ServiceId} actualizado", serviceId);

        return MapToResponse(service);
    }

    public async Task DeleteAsync(int serviceId)
    {
        var service = await _context.Services.FindAsync(serviceId)
            ?? throw new KeyNotFoundException($"Servicio con ID {serviceId} no encontrado.");

        service.Active = false;
        service.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Servicio {ServiceId} eliminado (logical delete)", serviceId);
    }

    private static ServiceResponseDto MapToResponse(Service s) => new()
    {
        ServiceId = s.Serviceid,
        ServiceName = s.Servicename,
        ServiceDescription = s.Servicedescription,
        SpeedMbps = s.SpeedMbps,
        Price = s.Price,
        Active = s.Active,
        CreatedAt = s.CreatedAt,
        Devices = s.Devices
            .Where(d => d.DeletedAt == null)
            .Select(d => new DeviceResponseDto
            {
                DeviceId = d.Deviceid,
                DeviceName = d.Devicename,
                ServiceId = d.ServiceServiceid,
                ServiceName = s.Servicename,
                Active = d.Active,
                CreatedAt = d.CreatedAt
            })
            .ToList()
    };
}
