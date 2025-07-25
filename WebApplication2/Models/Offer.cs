using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class Offer
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public Guid AnimalId { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal BuyingPrice { get; set; }
        
        public decimal Profit => SellingPrice - BuyingPrice;
        
        public bool IsActive { get; set; } = true;
        public bool IsSold { get; set; } = false;
        
        [Required]
        public string CreatedById { get; set; } = string.Empty; // DataEntry user
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Animal Animal { get; set; } = null!;
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}