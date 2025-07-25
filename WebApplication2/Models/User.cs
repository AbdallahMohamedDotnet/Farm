using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Farm? Farm { get; set; }
        public virtual ICollection<UserOtp> UserOtps { get; set; } = new List<UserOtp>();
    }
}