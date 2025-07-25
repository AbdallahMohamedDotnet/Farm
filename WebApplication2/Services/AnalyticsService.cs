using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IAuditService _auditService;
        
        public AnalyticsService(ApplicationDbContext context, UserManager<User> userManager, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
        }
        
        public async Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddMonths(-12);
            endDate ??= DateTime.UtcNow;
            
            var invoicesQuery = _context.Invoices
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate);
            
            var allInvoices = await invoicesQuery.ToListAsync();
            var paidInvoices = allInvoices.Where(i => i.Status == InvoiceStatus.Paid).ToList();
            
            var totalSales = paidInvoices.Sum(i => i.TotalAmount);
            var totalBuyingCost = paidInvoices.Sum(i => i.BuyingCost);
            var totalProfit = totalSales - totalBuyingCost;
            var profitMargin = totalSales > 0 ? (totalProfit / totalSales) * 100 : 0;
            
            // Monthly sales data
            var monthlySales = paidInvoices
                .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month })
                .Select(g => new MonthlySalesDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                    Sales = g.Sum(i => i.TotalAmount),
                    Profit = g.Sum(i => i.Profit),
                    InvoiceCount = g.Count()
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();
            
            // Top selling animals
            var topSellingAnimals = await _context.Invoices
                .Include(i => i.Offer)
                    .ThenInclude(o => o.Animal)
                .Where(i => i.Status == InvoiceStatus.Paid && i.CreatedAt >= startDate && i.CreatedAt <= endDate)
                .GroupBy(i => i.Offer.Animal.Type)
                .Select(g => new TopSellingAnimalDto
                {
                    AnimalType = g.Key,
                    TotalSold = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.TotalAmount),
                    TotalProfit = g.Sum(i => i.Profit)
                })
                .OrderByDescending(t => t.TotalRevenue)
                .Take(10)
                .ToListAsync();
            
            return new SalesAnalyticsDto
            {
                TotalSales = totalSales,
                TotalBuyingCost = totalBuyingCost,
                TotalProfit = totalProfit,
                ProfitMargin = profitMargin,
                TotalInvoices = allInvoices.Count,
                PaidInvoices = paidInvoices.Count,
                PendingInvoices = allInvoices.Count(i => i.Status == InvoiceStatus.Pending),
                MonthlySales = monthlySales,
                TopSellingAnimals = topSellingAnimals
            };
        }
        
        public async Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Offer)
                    .ThenInclude(o => o.Animal)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            
            var responseDtos = new List<InvoiceResponseDto>();
            foreach (var invoice in invoices)
            {
                responseDtos.Add(MapToInvoiceResponseDto(invoice));
            }
            
            return responseDtos;
        }
        
        public async Task<IEnumerable<OfferResponseDto>> GetAllOffersWithSalesAsync()
        {
            var offers = await _context.Offers
                .Include(o => o.Animal)
                .Include(o => o.CreatedBy)
                .Include(o => o.Invoices)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            
            var responseDtos = new List<OfferResponseDto>();
            foreach (var offer in offers)
            {
                responseDtos.Add(MapToOfferResponseDto(offer));
            }
            
            return responseDtos;
        }
        
        public async Task<ApiResponseDto> AssignDataEntryRoleAsync(string email, string superAdminId)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new ApiResponseDto { Message = "User not found with this email", Success = false };
            }
            
            if (!user.EmailConfirmed)
            {
                return new ApiResponseDto { Message = "User email is not confirmed", Success = false };
            }
            
            if (await _userManager.IsInRoleAsync(user, UserRoles.DataEntry))
            {
                return new ApiResponseDto { Message = "User already has DataEntry role", Success = false };
            }
            
            // Remove Customer role if exists
            if (await _userManager.IsInRoleAsync(user, UserRoles.Customer))
            {
                await _userManager.RemoveFromRoleAsync(user, UserRoles.Customer);
            }
            
            // Add DataEntry role
            var result = await _userManager.AddToRoleAsync(user, UserRoles.DataEntry);
            if (!result.Succeeded)
            {
                return new ApiResponseDto 
                { 
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    Success = false 
                };
            }
            
            await _auditService.LogActionAsync(superAdminId, "AssignDataEntryRole", "User", user.Id,
                $"Assigned DataEntry role to user {user.Email}");
            
            return new ApiResponseDto { Message = $"DataEntry role assigned successfully to {user.Email}" };
        }
        
        private InvoiceResponseDto MapToInvoiceResponseDto(Invoice invoice)
        {
            return new InvoiceResponseDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = $"{invoice.Customer.FirstName} {invoice.Customer.LastName}",
                CustomerEmail = invoice.Customer.Email!,
                OfferTitle = invoice.Offer.Title,
                AnimalName = invoice.Offer.Animal.Name,
                AnimalType = invoice.Offer.Animal.Type,
                Quantity = invoice.Quantity,
                UnitPrice = invoice.UnitPrice,
                TotalAmount = invoice.TotalAmount,
                BuyingCost = invoice.BuyingCost,
                Profit = invoice.Profit,
                Notes = invoice.Notes,
                Status = invoice.Status.ToString(),
                CreatedAt = invoice.CreatedAt,
                PaidAt = invoice.PaidAt
            };
        }
        
        private OfferResponseDto MapToOfferResponseDto(Offer offer)
        {
            return new OfferResponseDto
            {
                Id = offer.Id,
                Title = offer.Title,
                Description = offer.Description,
                AnimalId = offer.AnimalId,
                AnimalName = offer.Animal.Name,
                AnimalType = offer.Animal.Type,
                SellingPrice = offer.SellingPrice,
                BuyingPrice = offer.BuyingPrice,
                Profit = offer.Profit,
                IsActive = offer.IsActive,
                IsSold = offer.IsSold,
                CreatedByName = $"{offer.CreatedBy.FirstName} {offer.CreatedBy.LastName}",
                CreatedAt = offer.CreatedAt
            };
        }
    }
}