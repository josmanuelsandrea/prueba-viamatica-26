using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViamaticaApi.Application.DTOs.Payments;
using ViamaticaApi.Application.Interfaces;
using ViamaticaApi.Domain.Entities;
using ViamaticaApi.Infrastructure.Data;

namespace ViamaticaApi.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(AppDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetAllAsync(int? contractId, DateTime? date)
    {
        var query = _context.Payments
            .Include(p => p.Contract).ThenInclude(c => c.Service)
            .Include(p => p.Client)
            .AsQueryable();

        if (contractId.HasValue)
            query = query.Where(p => p.ContractContractid == contractId.Value);

        if (date.HasValue)
            query = query.Where(p => p.Paydate.Date == date.Value.Date);

        var payments = await query.OrderByDescending(p => p.Paydate).ToListAsync();
        return payments.Select(MapToResponse);
    }

    public async Task<PaymentResponseDto> GetByIdAsync(int paymentId)
    {
        var payment = await LoadPaymentAsync(paymentId)
            ?? throw new KeyNotFoundException($"Pago con ID {paymentId} no encontrado.");

        return MapToResponse(payment);
    }

    public async Task<PaymentResponseDto> CreateAsync(CreatePaymentDto dto)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Contractid == dto.ContractId)
            ?? throw new KeyNotFoundException($"Contrato con ID {dto.ContractId} no encontrado.");

        if (contract.StatuscontractStatusid == "CAN")
            throw new InvalidOperationException("No se puede registrar un pago para un contrato cancelado.");

        var payment = new Payment
        {
            ContractContractid = dto.ContractId,
            ClientClientid = contract.ClientClientid,
            MethodpaymentMethodpaymentid = contract.MethodpaymentMethodpaymentid,
            Amount = dto.Amount,
            AttentionAttentionid = dto.AttentionId > 0 ? dto.AttentionId : null,
            Paydate = DateTime.UtcNow,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pago {PaymentId} de {Amount} registrado para contrato {ContractId}",
            payment.Paymentid, dto.Amount, dto.ContractId);

        return await GetByIdAsync(payment.Paymentid);
    }

    private async Task<Payment?> LoadPaymentAsync(int paymentId)
    {
        return await _context.Payments
            .Include(p => p.Contract).ThenInclude(c => c.Service)
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Paymentid == paymentId);
    }

    private static PaymentResponseDto MapToResponse(Payment p) => new()
    {
        PaymentId = p.Paymentid,
        ContractId = p.ContractContractid,
        ClientId = p.ClientClientid,
        ClientName = p.Client != null ? $"{p.Client.Name} {p.Client.Lastname}" : string.Empty,
        ServiceName = p.Contract?.Service?.Servicename ?? string.Empty,
        Amount = p.Amount,
        AttentionId = p.AttentionAttentionid ?? 0,
        CreatedAt = p.CreatedAt
    };
}
