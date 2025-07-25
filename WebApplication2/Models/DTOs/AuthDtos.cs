using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string LastName { get; set; } = string.Empty;
    }
    
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    
    public class ConfirmEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = string.Empty;
    }
    
    public class ResendOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Purpose { get; set; } = string.Empty; // EmailConfirmation, PasswordReset
    }
    
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public bool RequiresEmailConfirmation { get; set; } = false;
        public string? UserName { get; set; } // For login response - only return name
        public UserInfoDto? UserInfo { get; set; }
    }
    
    public class UserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
    }
    
    // New DTO for pending registrations
    public class PendingRegistrationDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30); // 30 minutes to confirm
    }
}