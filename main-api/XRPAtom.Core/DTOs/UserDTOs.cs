using System;
using System.ComponentModel.DataAnnotations;
using XRPAtom.Core.Domain;

namespace XRPAtom.Core.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public string Organization { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        public UserWalletDto Wallet { get; set; }
        public int DeviceCount { get; set; }
        public int ActiveEventCount { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [Required]
        public UserRole Role { get; set; }
        
        [StringLength(200)]
        public string Organization { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
    }

    public class UpdateUserDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [StringLength(200)]
        public string Organization { get; set; }
    }

    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }

    public class UserRegistrationDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
        
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [Required]
        public UserRole Role { get; set; }
        
        [StringLength(200)]
        public string Organization { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
        
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}