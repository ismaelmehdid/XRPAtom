using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.Blockchain.Services
{
    public class UserWalletService : IUserWalletService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserWalletService> _logger;

        public UserWalletService(ApplicationDbContext context, ILogger<UserWalletService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserWalletDto> GetWalletByUserIdAsync(string userId)
        {
            try
            {
                var wallet = await _context.UserWallets
                    .Include(w => w.User)
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return null;
                }

                return MapToWalletDto(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wallet for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserWalletDto> GetWalletByAddressAsync(string address)
        {
            try
            {
                var wallet = await _context.UserWallets
                    .Include(w => w.User)
                    .FirstOrDefaultAsync(w => w.Address == address);

                if (wallet == null)
                {
                    return null;
                }

                return MapToWalletDto(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wallet with address {Address}", address);
                throw;
            }
        }

        public async Task<UserWalletDto> CreateWalletAsync(string userId, string address)
        {
            try
            {
                // Check if user already has a wallet
                var existingWallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (existingWallet != null)
                {
                    throw new InvalidOperationException("User already has a wallet");
                }

                // Check if wallet address is already in use
                var addressInUse = await _context.UserWallets
                    .AnyAsync(w => w.Address == address);

                if (addressInUse)
                {
                    throw new InvalidOperationException("Wallet address already in use");
                }

                // Create new wallet
                var wallet = new UserWallet
                {
                    UserId = userId,
                    Address = address,
                    Balance = 0,
                    AtomTokenBalance = 0,
                    TotalRewardsClaimed = 0,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.UserWallets.Add(wallet);
                await _context.SaveChangesAsync();

                return MapToWalletDto(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wallet for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateWalletBalanceAsync(string userId, decimal newBalance)
        {
            try
            {
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return false;
                }

                wallet.Balance = newBalance;
                wallet.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating balance for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateTokenBalanceAsync(string userId, decimal newBalance)
        {
            try
            {
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return false;
                }

                wallet.AtomTokenBalance = newBalance;
                wallet.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating token balance for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RefreshWalletBalancesAsync(string userId)
        {
            try
            {
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return false;
                }

                // In a real application, you would query the XRP Ledger for the current balance
                // For now, we'll just update the timestamp
                wallet.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing wallet balances for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> SetWalletActiveStatusAsync(string userId, bool active)
        {
            try
            {
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return false;
                }

                wallet.IsActive = active;
                wallet.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating wallet active status for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateTotalRewardsClaimedAsync(string userId, decimal additionalRewards)
        {
            try
            {
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return false;
                }

                wallet.TotalRewardsClaimed += additionalRewards;
                wallet.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating total rewards claimed for user {UserId}", userId);
                throw;
            }
        }

        private UserWalletDto MapToWalletDto(UserWallet wallet)
        {
            if (wallet == null) return null;

            return new UserWalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Address = wallet.Address,
                Balance = wallet.Balance,
                AtomTokenBalance = wallet.AtomTokenBalance,
                TotalRewardsClaimed = wallet.TotalRewardsClaimed,
                CreatedAt = wallet.CreatedAt,
                LastUpdated = wallet.LastUpdated,
                IsActive = wallet.IsActive
            };
        }
    }
}