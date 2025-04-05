using XRPAtom.Core.DTOs;

namespace XRPAtom.Blockchain.Interfaces
{
    public interface IUserWalletService
    {
        /// <summary>
        /// Gets a user's wallet by user ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The user's wallet if found</returns>
        Task<UserWalletDto> GetWalletByUserIdAsync(string userId);
        
        /// <summary>
        /// Gets a wallet by its XRPL address
        /// </summary>
        /// <param name="address">The XRPL address</param>
        /// <returns>The wallet if found</returns>
        Task<UserWalletDto> GetWalletByAddressAsync(string address);
        
        /// <summary>
        /// Creates a wallet for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="address">The XRPL address</param>
        /// <returns>The created wallet</returns>
        Task<UserWalletDto> CreateWalletAsync(string userId, string address);
        
        /// <summary>
        /// Updates a wallet's XRP balance
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="newBalance">The new balance</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> UpdateWalletBalanceAsync(string userId, decimal newBalance);
        
        /// <summary>
        /// Updates a wallet's ATOM token balance
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="newBalance">The new ATOM token balance</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> UpdateTokenBalanceAsync(string userId, decimal newBalance);
        
        /// <summary>
        /// Refreshes a wallet's balances from the XRP Ledger
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if refreshed successfully</returns>
        Task<bool> RefreshWalletBalancesAsync(string userId);
        
        /// <summary>
        /// Activates or deactivates a wallet
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="active">Whether the wallet should be active</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> SetWalletActiveStatusAsync(string userId, bool active);
        
        /// <summary>
        /// Updates the total rewards claimed by a wallet
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="additionalRewards">The additional rewards claimed</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> UpdateTotalRewardsClaimedAsync(string userId, decimal additionalRewards);
    }
}