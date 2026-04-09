using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViamaticaApi.Application.DTOs.Contracts;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Domain.Entities;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class ContractService : IContractService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ContractService> _logger;

    // Status codes as constants
    private const string StatusVigente = "VIG";
    private const string StatusSustituido = "SUS";
    private const string StatusCancelado = "CAN";
    private const string StatusRenovado = "REN";

    public ContractService(AppDbContext context, ILogger<ContractService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ContractResponseDto>> GetAllAsync(int? clientId, string? statusCode)
    {
        var query = _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.Service)
            .Include(c => c.MethodPayment)
            .Include(c => c.StatusContract)
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(c => c.ClientClientid == clientId.Value);

        if (!string.IsNullOrEmpty(statusCode))
            query = query.Where(c => c.StatuscontractStatusid == statusCode.ToUpper());

        var contracts = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return contracts.Select(MapToResponse);
    }

    public async Task<ContractResponseDto> GetByIdAsync(int contractId)
    {
        var contract = await LoadContractAsync(contractId)
            ?? throw new KeyNotFoundException($"Contrato con ID {contractId} no encontrado.");

        return MapToResponse(contract);
    }

    public async Task<ContractResponseDto> CreateAsync(CreateContractDto dto)
    {
        var client = await _context.Clients.FindAsync(dto.ClientId)
            ?? throw new KeyNotFoundException($"Cliente con ID {dto.ClientId} no encontrado.");

        var service = await _context.Services.FindAsync(dto.ServiceId)
            ?? throw new KeyNotFoundException($"Servicio con ID {dto.ServiceId} no encontrado.");

        var methodPayment = await _context.MethodPayments.FindAsync(dto.MethodPaymentId)
            ?? throw new KeyNotFoundException($"Método de pago con ID {dto.MethodPaymentId} no encontrado.");

        // Validate no active contract exists for this client
        var hasActive = await _context.Contracts
            .AnyAsync(c => c.ClientClientid == dto.ClientId && c.StatuscontractStatusid == StatusVigente);

        if (hasActive)
            throw new InvalidOperationException("El cliente ya tiene un contrato vigente.");

        var contract = new Contract
        {
            ClientClientid = dto.ClientId,
            ServiceServiceid = dto.ServiceId,
            MethodpaymentMethodpaymentid = dto.MethodPaymentId,
            StatuscontractStatusid = StatusVigente,
            Startdate = dto.StartDate,
            Enddate = dto.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Contrato {ContractId} creado para cliente {ClientId} con servicio {ServiceId}",
            contract.Contractid, dto.ClientId, dto.ServiceId);

        return await GetByIdAsync(contract.Contractid);
    }

    public async Task<ContractResponseDto> ChangeServiceAsync(int contractId, ChangeServiceDto dto)
    {
        var oldContract = await LoadContractAsync(contractId)
            ?? throw new KeyNotFoundException($"Contrato con ID {contractId} no encontrado.");

        if (oldContract.StatuscontractStatusid != StatusVigente)
            throw new InvalidOperationException("Solo se puede cambiar el servicio de un contrato vigente.");

        var newService = await _context.Services.FindAsync(dto.NewServiceId)
            ?? throw new KeyNotFoundException($"Servicio con ID {dto.NewServiceId} no encontrado.");

        // Mark old contract as substituted
        oldContract.StatuscontractStatusid = StatusSustituido;
        oldContract.UpdatedAt = DateTime.UtcNow;

        // Create new contract with REN status preserving end date
        var newContract = new Contract
        {
            ClientClientid = oldContract.ClientClientid,
            ServiceServiceid = dto.NewServiceId,
            MethodpaymentMethodpaymentid = oldContract.MethodpaymentMethodpaymentid,
            StatuscontractStatusid = StatusRenovado,
            Startdate = DateTime.UtcNow,
            Enddate = oldContract.Enddate,
            ParentContractid = contractId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contracts.Add(newContract);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Contrato {OldContractId} sustituido. Nuevo contrato {NewContractId} con servicio {ServiceId}",
            contractId, newContract.Contractid, dto.NewServiceId);

        return await GetByIdAsync(newContract.Contractid);
    }

    public async Task<ContractResponseDto> ChangePaymentMethodAsync(int contractId, ChangePaymentMethodDto dto)
    {
        var contract = await LoadContractAsync(contractId)
            ?? throw new KeyNotFoundException($"Contrato con ID {contractId} no encontrado.");

        if (contract.StatuscontractStatusid != StatusVigente)
            throw new InvalidOperationException("Solo se puede cambiar el método de pago de un contrato vigente.");

        var methodPayment = await _context.MethodPayments.FindAsync(dto.MethodPaymentId)
            ?? throw new KeyNotFoundException($"Método de pago con ID {dto.MethodPaymentId} no encontrado.");

        contract.MethodpaymentMethodpaymentid = dto.MethodPaymentId;
        contract.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Método de pago del contrato {ContractId} actualizado a {MethodPaymentId}",
            contractId, dto.MethodPaymentId);

        return await GetByIdAsync(contractId);
    }

    public async Task<ContractResponseDto> CancelAsync(int contractId)
    {
        var contract = await LoadContractAsync(contractId)
            ?? throw new KeyNotFoundException($"Contrato con ID {contractId} no encontrado.");

        if (contract.StatuscontractStatusid == StatusCancelado)
            throw new InvalidOperationException("El contrato ya está cancelado.");

        contract.StatuscontractStatusid = StatusCancelado;
        contract.Enddate = DateTime.UtcNow;
        contract.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Contrato {ContractId} cancelado", contractId);

        return await GetByIdAsync(contractId);
    }

    private async Task<Contract?> LoadContractAsync(int contractId)
    {
        return await _context.Contracts
            .Include(c => c.Client)
            .Include(c => c.Service)
            .Include(c => c.MethodPayment)
            .Include(c => c.StatusContract)
            .FirstOrDefaultAsync(c => c.Contractid == contractId);
    }

    private static ContractResponseDto MapToResponse(Contract c) => new()
    {
        ContractId = c.Contractid,
        ClientId = c.ClientClientid,
        ClientName = c.Client != null ? $"{c.Client.Name} {c.Client.Lastname}" : string.Empty,
        ServiceId = c.ServiceServiceid,
        ServiceName = c.Service?.Servicename ?? string.Empty,
        MethodPaymentId = c.MethodpaymentMethodpaymentid,
        MethodPaymentName = c.MethodPayment?.Description ?? string.Empty,
        StatusCode = c.StatuscontractStatusid,
        StatusDescription = c.StatusContract?.Description ?? string.Empty,
        StartDate = c.Startdate,
        EndDate = c.Enddate ?? DateTime.MinValue,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
