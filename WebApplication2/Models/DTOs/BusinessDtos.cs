using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models.DTOs
{
    // Offer DTOs
    public class CreateOfferDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public Guid AnimalId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Selling price must be greater than 0")]
        public decimal SellingPrice { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Buying price must be greater than 0")]
        public decimal BuyingPrice { get; set; }
    }
    
    public class UpdateOfferDto
    {
        [StringLength(100, MinimumLength = 1)]
        public string? Title { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Selling price must be greater than 0")]
        public decimal? SellingPrice { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Buying price must be greater than 0")]
        public decimal? BuyingPrice { get; set; }
        
        public bool? IsActive { get; set; }
    }
    
    public class OfferResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid AnimalId { get; set; }
        public string AnimalName { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public decimal BuyingPrice { get; set; }
        public decimal Profit { get; set; }
        public bool IsActive { get; set; }
        public bool IsSold { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
    
    // Stock DTOs
    public class CreateStockDto
    {
        [Required]
        public Guid AnimalId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Buying price must be greater than 0")]
        public decimal BuyingPrice { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Selling price must be greater than 0")]
        public decimal SellingPrice { get; set; }
        
        [StringLength(200)]
        public string? Notes { get; set; }
    }
    
    public class StockResponseDto
    {
        public Guid Id { get; set; }
        public Guid AnimalId { get; set; }
        public string AnimalName { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal BuyingPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal ProfitPerUnit { get; set; }
        public string? Notes { get; set; }
        public string AddedByName { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
    }
    
    // Invoice DTOs
    public class CreateInvoiceDto
    {
        [Required]
        public Guid OfferId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
    
    public class InvoiceResponseDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string OfferTitle { get; set; } = string.Empty;
        public string AnimalName { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal BuyingCost { get; set; }
        public decimal Profit { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
    
    // Sales Analytics DTOs for SuperAdmin
    public class SalesAnalyticsDto
    {
        public decimal TotalSales { get; set; }
        public decimal TotalBuyingCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public List<MonthlySalesDto> MonthlySales { get; set; } = new();
        public List<TopSellingAnimalDto> TopSellingAnimals { get; set; } = new();
    }
    
    public class MonthlySalesDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public decimal Profit { get; set; }
        public int InvoiceCount { get; set; }
    }
    
    public class TopSellingAnimalDto
    {
        public string AnimalType { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
    }
}