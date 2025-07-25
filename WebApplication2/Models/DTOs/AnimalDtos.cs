using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models.DTOs
{
    public class CreateAnimalDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [RegularExpression("^(Camel|Cow|Sheep|Goat)$", ErrorMessage = "Type must be Camel, Cow, Sheep, or Goat")]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 300, ErrorMessage = "Age must be between 1 and 300 months")]
        public int Age { get; set; }
        
        public Guid? CategoryId { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Weight must be a positive value")]
        public decimal Weight { get; set; }
        
        [StringLength(50)]
        public string? Breed { get; set; }
    }
    
    public class UpdateAnimalDto
    {
        [StringLength(50, MinimumLength = 1)]
        public string? Name { get; set; }
        
        [RegularExpression("^(Camel|Cow|Sheep|Goat)$", ErrorMessage = "Type must be Camel, Cow, Sheep, or Goat")]
        public string? Type { get; set; }
        
        [Range(1, 300, ErrorMessage = "Age must be between 1 and 300 months")]
        public int? Age { get; set; }
        
        public Guid? CategoryId { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Weight must be a positive value")]
        public decimal? Weight { get; set; }
        
        [StringLength(50)]
        public string? Breed { get; set; }
    }
    
    public class AnimalResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsFed { get; set; }
        public bool IsGroomed { get; set; }
        public bool IsSacrificed { get; set; }
        public bool IsEligibleForSacrifice { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public decimal Weight { get; set; }
        public string? Breed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
    }
    
    public class ApiResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public object? Data { get; set; }
    }
}