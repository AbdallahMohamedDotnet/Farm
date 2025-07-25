using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class Invoice
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string InvoiceNumber { get; set; } = string.Empty;
        
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        
        [Required]
        public Guid OfferId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal BuyingCost { get; set; }
        
        public decimal Profit => TotalAmount - BuyingCost;
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        
        // Navigation properties
        public virtual User Customer { get; set; } = null!;
        public virtual Offer Offer { get; set; } = null!;
    }
    
    public enum InvoiceStatus
    {
        Pending = 0,
        Paid = 1,
        Cancelled = 2
    }
}