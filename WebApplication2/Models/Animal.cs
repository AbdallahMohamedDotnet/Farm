using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class Animal
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // Camel, Cow, Sheep, Goat
        
        [Range(0, 300)]
        public int Age { get; set; } // Age in months
        
        public bool IsFed { get; set; } = false;
        public bool IsGroomed { get; set; } = false;
        public bool IsSacrificed { get; set; } = false;
        
        [Required]
        public Guid FarmId { get; set; }
        
        public Guid? CategoryId { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal Weight { get; set; }
        
        [MaxLength(50)]
        public string? Breed { get; set; }
        
        // Pricing information
        [Range(0, double.MaxValue)]
        public decimal BuyingPrice { get; set; } = 0;
        
        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; } = 0;
        
        public decimal ProfitMargin => SellingPrice > 0 ? SellingPrice - BuyingPrice : 0;
        
        public bool IsForSale { get; set; } = false;
        
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } = 1;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Farm Farm { get; set; } = null!;
        public virtual Category? Category { get; set; }
        public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        
        // Method to check if animal is eligible for sacrifice
        public bool IsEligibleForSacrifice()
        {
            if (IsSacrificed) return false;
            
            return Type.ToLower() switch
            {
                "camel" => Age >= 60, // 5 years
                "cow" => Age >= 24,   // 2 years
                "goat" => Age >= 12,  // 1 year
                "sheep" => Age >= 6,  // 6 months
                _ => false
            };
        }
    }
}