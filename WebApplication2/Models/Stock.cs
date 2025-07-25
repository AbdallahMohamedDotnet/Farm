using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class Stock
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid AnimalId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal BuyingPrice { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; }
        
        public decimal ProfitPerUnit => SellingPrice - BuyingPrice;
        
        [MaxLength(200)]
        public string? Notes { get; set; }
        
        [Required]
        public string AddedById { get; set; } = string.Empty; // DataEntry user
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Animal Animal { get; set; } = null!;
        public virtual User AddedBy { get; set; } = null!;
    }
}