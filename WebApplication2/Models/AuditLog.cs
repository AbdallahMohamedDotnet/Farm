using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;
        
        public Guid? EntityId { get; set; }
        
        [MaxLength(1000)]
        public string? Details { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}