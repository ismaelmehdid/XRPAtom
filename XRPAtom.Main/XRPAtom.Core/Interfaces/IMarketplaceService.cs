using System.Collections.Generic;
using System.Threading.Tasks;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;

namespace XRPAtom.Core.Interfaces
{
    public interface IMarketplaceService
    {
        // Listings
        Task<MarketplaceListingDto> GetListingByIdAsync(string listingId);
        
        Task<IEnumerable<MarketplaceListingDto>> GetListingsAsync(string type, int page = 1, int pageSize = 10);
        
        Task<IEnumerable<MarketplaceListingDto>> GetUserListingsAsync(string userId, int page = 1, int pageSize = 10);
        
        Task<MarketplaceListingDto> CreateListingAsync(CreateMarketplaceListingDto createListingDto);
        
        Task<MarketplaceListingDto> UpdateListingAsync(string listingId, UpdateMarketplaceListingDto updateListingDto);
        
        Task<bool> DeleteListingAsync(string listingId);
        
        Task<bool> UpdateListingStatusAsync(string listingId, string status);
        
        Task<bool> UpdateListingBlockchainReferenceAsync(string listingId, string blockchainReference);
        
        // Transactions
        Task<MarketplaceTransactionDto> GetTransactionByIdAsync(string transactionId);
        
        Task<IEnumerable<MarketplaceTransactionDto>> GetTransactionsByListingAsync(string listingId);
        
        Task<IEnumerable<MarketplaceTransactionDto>> GetUserTransactionsAsync(string userId, int page = 1, int pageSize = 10);
        
        Task<MarketplaceTransactionDto> CreateTransactionAsync(CreateMarketplaceTransactionDto createTransactionDto);
        
        Task<bool> UpdateTransactionStatusAsync(string transactionId, string status);
        
        Task<bool> SetTransactionHashAsync(string transactionId, string transactionHash);
        
        // Market Data
        Task<decimal> GetAveragePriceAsync(string type);
        
        Task<Dictionary<string, decimal>> GetMarketVolumeAsync(int days);
        
        Task<IEnumerable<MarketplaceListingDto>> GetActiveListingsByPriceRangeAsync(string type, decimal minPrice, decimal maxPrice);
        
        Task<IEnumerable<MarketplaceListingDto>> GetMatchingListingsAsync(string type, decimal amount, decimal maxPrice);
        
        // Validation
        Task<bool> IsUserOwnedListingAsync(string listingId, string userId);
        
        Task<bool> CanUserPurchaseListingAsync(string listingId, string userId, decimal amount);
    }
}