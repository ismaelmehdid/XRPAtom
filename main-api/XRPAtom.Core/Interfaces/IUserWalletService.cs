using XRPAtom.Core.DTOs;

namespace XRPAtom.Core.Interfaces
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
        /// Creates a wallet for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="address">The XRPL address</param>
        /// <returns>The created wallet</returns>
        Task<UserWalletDto> CreateWalletAsync(string userId, string address);
        
        /// <summary>
        /// Updates the total rewards claimed by a wallet
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="additionalRewards">The additional rewards claimed</param>
        /// <returns>True if updated successfully</returns>
        Task<bool> UpdateTotalRewardsClaimedAsync(string userId, decimal additionalRewards);
    }
}