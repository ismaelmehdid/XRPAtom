using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.Infrastructure.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(ApplicationDbContext context, ILogger<PasswordService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                // Validate input
                if (changePasswordDto == null)
                {
                    throw new ArgumentNullException(nameof(changePasswordDto), "Password change details cannot be null");
                }

                // Find the user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                // Verify current password
                var currentPasswordHash = HashPassword(changePasswordDto.CurrentPassword);
                if (user.PasswordHash != currentPasswordHash)
                {
                    throw new InvalidOperationException("Current password is incorrect");
                }

                // Update password
                user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
                await _context.SaveChangesAsync();

                // Log password change event
                await LogPasswordChangeEventAsync(userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ValidatePasswordAsync(User user, string password)
        {
            try
            {
                // Find the user in the database to get the current password hash
                var dbUser = await _context.Users.FindAsync(user.Id);
                if (dbUser == null)
                {
                    return false;
                }

                // Compare hashed passwords
                var passwordHash = HashPassword(password);
                return dbUser.PasswordHash == passwordHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password for user {UserId}", user.Id);
                return false;
            }
        }

        private async Task LogPasswordChangeEventAsync(string userId)
        {
            await Task.CompletedTask;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}