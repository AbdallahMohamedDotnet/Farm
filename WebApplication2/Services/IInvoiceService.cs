using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto, string customerId);
        Task<IEnumerable<InvoiceResponseDto>> GetCustomerInvoicesAsync(string customerId);
        Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId, string customerId);
        Task<ApiResponseDto> PayInvoiceAsync(Guid invoiceId, string customerId);
        Task<ApiResponseDto> CancelInvoiceAsync(Guid invoiceId, string customerId);
    }
}