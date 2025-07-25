using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class UserOtp
    {
        public Guid Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(6)]
        public string OtpCode { get; set; } = string.Empty;
        
        [Required]
        public string Purpose { get; set; } = string.Empty; // EmailConfirmation, PasswordReset, etc.
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        
        // Navigation property - nullable because it won't exist for pending registrations
        public virtual User? User { get; set; }
    }
}