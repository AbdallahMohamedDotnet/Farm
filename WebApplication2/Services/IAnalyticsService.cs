using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public interface IAnalyticsService
    {
        Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync();
        Task<IEnumerable<OfferResponseDto>> GetAllOffersWithSalesAsync();
        Task<ApiResponseDto> AssignDataEntryRoleAsync(string email, string superAdminId);
    }
}