using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        
        public InvoiceService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }
        
        public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto, string customerId)
        {
            var offer = await _context.Offers
                .Include(o => o.Animal)
                .Include(o => o.CreatedBy)
                .FirstOrDefaultAsync(o => o.Id == createInvoiceDto.OfferId && o.IsActive && !o.IsSold);
            
            if (offer == null)
            {
                throw new InvalidOperationException("Offer not found or not available for purchase");
            }
            
            // Generate unique invoice number
            var invoiceNumber = await GenerateInvoiceNumberAsync();
            
            var totalAmount = offer.SellingPrice * createInvoiceDto.Quantity;
            var buyingCost = offer.BuyingPrice * createInvoiceDto.Quantity;
            
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber,
                CustomerId = customerId,
                OfferId = createInvoiceDto.OfferId,
                Quantity = createInvoiceDto.Quantity,
                UnitPrice = offer.SellingPrice,
                TotalAmount = totalAmount,
                BuyingCost = buyingCost,
                Notes = createInvoiceDto.Notes,
                Status = InvoiceStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Invoices.Add(invoice);
            
            // Mark offer as sold if quantity matches
            if (createInvoiceDto.Quantity >= offer.Animal.StockQuantity)
            {
                offer.IsSold = true;
                offer.Animal.IsForSale = false;
                offer.Animal.StockQuantity = 0;
            }
            else
            {
                offer.Animal.StockQuantity -= createInvoiceDto.Quantity;
            }
            
            await _context.SaveChangesAsync();
            
            await _auditService.LogActionAsync(customerId, "CreateInvoice", "Invoice", invoice.Id.ToString(),
                $"Created invoice {invoiceNumber} for offer '{offer.Title}'");
            
            return await MapToResponseDto(invoice);
        }
        
        public async Task<IEnumerable<InvoiceResponseDto>> GetCustomerInvoicesAsync(string customerId)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Offer)
                    .ThenInclude(o => o.Animal)
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            
            var responseDtos = new List<InvoiceResponseDto>();
            foreach (var invoice in invoices)
            {
                responseDtos.Add(await MapToResponseDto(invoice));
            }
            
            return responseDtos;
        }
        
        public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid invoiceId, string customerId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Offer)
                    .ThenInclude(o => o.Animal)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.CustomerId == customerId);
            
            return invoice != null ? await MapToResponseDto(invoice) : null;
        }
        
        public async Task<ApiResponseDto> PayInvoiceAsync(Guid invoiceId, string customerId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.CustomerId == customerId);
            
            if (invoice == null)
            {
                return new ApiResponseDto { Message = "Invoice not found", Success = false };
            }
            
            if (invoice.Status == InvoiceStatus.Paid)
            {
                return new ApiResponseDto { Message = "Invoice is already paid", Success = false };
            }
            
            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                return new ApiResponseDto { Message = "Cannot pay a cancelled invoice", Success = false };
            }
            
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            await _auditService.LogActionAsync(customerId, "PayInvoice", "Invoice", invoice.Id.ToString(),
                $"Paid invoice {invoice.InvoiceNumber}");
            
            return new ApiResponseDto { Message = "Invoice paid successfully" };
        }
        
        public async Task<ApiResponseDto> CancelInvoiceAsync(Guid invoiceId, string customerId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Offer)
                    .ThenInclude(o => o.Animal)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.CustomerId == customerId);
            
            if (invoice == null)
            {
                return new ApiResponseDto { Message = "Invoice not found", Success = false };
            }
            
            if (invoice.Status == InvoiceStatus.Paid)
            {
                return new ApiResponseDto { Message = "Cannot cancel a paid invoice", Success = false };
            }
            
            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                return new ApiResponseDto { Message = "Invoice is already cancelled", Success = false };
            }
            
            invoice.Status = InvoiceStatus.Cancelled;
            
            // Restore stock quantity
            invoice.Offer.Animal.StockQuantity += invoice.Quantity;
            if (invoice.Offer.IsSold && invoice.Offer.Animal.StockQuantity > 0)
            {
                invoice.Offer.IsSold = false;
                invoice.Offer.Animal.IsForSale = true;
            }
            
            await _context.SaveChangesAsync();
            
            await _auditService.LogActionAsync(customerId, "CancelInvoice", "Invoice", invoice.Id.ToString(),
                $"Cancelled invoice {invoice.InvoiceNumber}");
            
            return new ApiResponseDto { Message = "Invoice cancelled successfully" };
        }
        
        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var lastInvoice = await _context.Invoices
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();
            
            var nextNumber = 1;
            if (lastInvoice != null && lastInvoice.InvoiceNumber.StartsWith("INV"))
            {
                var numberPart = lastInvoice.InvoiceNumber.Substring(3);
                if (int.TryParse(numberPart, out var currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }
            
            return $"INV{nextNumber:D6}"; // INV000001, INV000002, etc.
        }
        
        private async Task<InvoiceResponseDto> MapToResponseDto(Invoice invoice)
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
    }
}