using ViamaticaApi.Application.DTOs.Payments;

namespace ViamaticaApi.Application.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponseDto>> GetAllAsync(int? contractId, DateTime? date);
    Task<PaymentResponseDto> GetByIdAsync(int paymentId);
    Task<PaymentResponseDto> CreateAsync(CreatePaymentDto dto);
}
