using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Wallet)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return null;
                }

                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Wallet)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return null;
                }

                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email {Email}", email);
                throw;
            }
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
                {
                    throw new InvalidOperationException("Email is already in use");
                }

                // Create a new user
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = createUserDto.Name,
                    Email = createUserDto.Email,
                    PhoneNumber = createUserDto.PhoneNumber,
                    Role = createUserDto.Role,
                    Organization = createUserDto.Organization,
                    CreatedAt = DateTime.UtcNow,
                    // For simplicity, we're storing a hashed password directly
                    // In a real app, you would use ASP.NET Core Identity or similar
                    PasswordHash = HashPassword(createUserDto.Password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return null;
                }

                // Update user properties
                if (!string.IsNullOrEmpty(updateUserDto.Name))
                {
                    user.Name = updateUserDto.Name;
                }

                if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber))
                {
                    user.PhoneNumber = updateUserDto.PhoneNumber;
                }

                if (!string.IsNullOrEmpty(updateUserDto.Organization))
                {
                    user.Organization = updateUserDto.Organization;
                }

                await _context.SaveChangesAsync();

                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<UserDto> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Wallet)
                    .Include(u => u.Devices)
                    .Include(u => u.EventParticipations)
                        .ThenInclude(p => p.Event)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return null;
                }

                var userDto = MapToUserDto(user);

                // Enrich with additional profile information
                userDto.DeviceCount = user.Devices?.Count ?? 0;
                userDto.ActiveEventCount = user.EventParticipations?
                    .Count(p => p.Event.Status == EventStatus.Active) ?? 0;

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(string userId, UserRole newRole)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.Role = newRole;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing role for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // In a real app, you might have an IsActive flag
                // For simplicity, we're not implementing actual deactivation logic here
                // user.IsActive = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ReactivateUserAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // In a real app, you might have an IsActive flag
                // For simplicity, we're not implementing actual reactivation logic here
                // user.IsActive = true;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateLastLoginAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role, int page = 1, int pageSize = 10)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.Role == role)
                    .OrderBy(u => u.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return users.Select(MapToUserDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users by role {Role}", role);
                throw;
            }
        }

        public async Task<int> GetUserDeviceCountAsync(string userId)
        {
            return await _context.Devices.CountAsync(d => d.UserId == userId);
        }

        public async Task<int> GetActiveEventCountAsync(string userId)
        {
            return await _context.EventParticipations
                .Where(p => p.UserId == userId)
                .Include(p => p.Event)
                .CountAsync(p => p.Event.Status == EventStatus.Active);
        }

        public async Task<decimal> GetTotalRewardsAsync(string userId)
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            return wallet?.TotalRewardsClaimed ?? 0;
        }

        public async Task<DateTime> GetRegistrationDateAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.CreatedAt ?? DateTime.MinValue;
        }

        // Helper methods
        private UserDto MapToUserDto(User user)
        {
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                Organization = user.Organization,
                EmailConfirmed = true, // In a real app, you would use actual data
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = true, // In a real app, you would use actual data
                Wallet = user.Wallet != null ? new UserWalletDto
                {
                    Id = user.Wallet.Id,
                    UserId = user.Wallet.UserId,
                    Address = user.Wallet.Address,
                    PublicKey = user.Wallet.PublicKey,
                    Balance = user.Wallet.Balance,
                    AtomTokenBalance = user.Wallet.AtomTokenBalance,
                    TotalRewardsClaimed = user.Wallet.TotalRewardsClaimed,
                    CreatedAt = user.Wallet.CreatedAt,
                    LastUpdated = user.Wallet.LastUpdated,
                    IsActive = user.Wallet.IsActive
                } : null,
                DeviceCount = user.Devices?.Count ?? 0,
                ActiveEventCount = 0 // Calculated separately when needed
            };
        }

        private string HashPassword(string password)
        {
            // WARNING: This is a simple hash for demonstration purposes only
            // In a real application, use a proper password hasher like BCrypt
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}