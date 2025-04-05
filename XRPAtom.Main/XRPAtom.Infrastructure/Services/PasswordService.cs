using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.Infrastructure.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IUserStore<User> _userStore;

        public PasswordService(
            UserManager<User> userManager, 
            ApplicationDbContext context,
            IUserStore<User> userStore)
        {
            _userManager = userManager;
            _context = context;
            _userStore = userStore;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            // Validate input
            if (changePasswordDto == null)
            {
                throw new ArgumentNullException(nameof(changePasswordDto), "Password change details cannot be null");
            }

            // Find the user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Change password using Identity framework
            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user, 
                changePasswordDto.CurrentPassword, 
                changePasswordDto.NewPassword
            );

            // Check if password change was successful
            if (!changePasswordResult.Succeeded)
            {
                // Collect and throw all error messages
                var errors = changePasswordResult.Errors.Select(e => e.Description);
                throw new InvalidOperationException(
                    $"Password change failed: {string.Join(", ", errors)}"
                );
            }

            // Log password change event
            await LogPasswordChangeEventAsync(userId);

            return true;
        }

        public async Task<bool> ValidatePasswordAsync(User user, string password)
        {
            // Use UserManager's built-in password validation
            var passwordValidators = _userManager.PasswordValidators;
            
            foreach (var validator in passwordValidators)
            {
                var validationResult = await validator.ValidateAsync(_userManager, user, password);
                
                // If any validator fails, return false
                if (!validationResult.Succeeded)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task LogPasswordChangeEventAsync(string userId)
        {
            // Placeholder for logging logic
            // This could involve saving to a database, sending a notification, etc.
            return;
        }
    }
}