using System.ComponentModel.DataAnnotations;

namespace XRPAtom.Core.DTOs
{
    public class MarketplaceListingDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // "buy" or "sell"
        public string Provider { get; set; }
        public string ProviderType { get; set; }
        public object AvailabilityWindow { get; set; } // Deserialized from JSON
        public decimal PricePerKwh { get; set; }
        public decimal MinKwh { get; set; }
        public decimal MaxKwh { get; set; }
        public string Status { get; set; }
        public string BlockchainReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserDto ProviderUser { get; set; }
        public bool IsOwner { get; set; } // Indicates if the requesting user is the owner
    }

    public class CreateMarketplaceListingDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [RegularExpression("^(buy|sell)$")]
        public string Type { get; set; }
        
        [Required]
        [StringLength(200)]
        public string AvailabilityWindow { get; set; }
        
        [Required]
        [Range(0.01, 100)]
        public decimal PricePerKwh { get; set; }
        
        [Required]
        [Range(0.1, 10000)]
        public decimal MinKwh { get; set; }
        
        [Required]
        [Range(0.1, 10000)]
        public decimal MaxKwh { get; set; }
        
        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }
    }

    public class UpdateMarketplaceListingDto
    {
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [StringLength(200)]
        public string AvailabilityWindow { get; set; }
        
        [Range(0.01, 100)]
        public decimal? PricePerKwh { get; set; }
        
        [Range(0.1, 10000)]
        public decimal? MinKwh { get; set; }
        
        [Range(0.1, 10000)]
        public decimal? MaxKwh { get; set; }
    }

    public class ListingStatusUpdateDto
    {
        [Required]
        [RegularExpression("^(active|inactive|completed|expired)$")]
        public string Status { get; set; }
    }

    public class MarketplaceTransactionDto
    {
        public string Id { get; set; }
        public string ListingId { get; set; }
        public string BuyerId { get; set; }
        public string SellerId { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string TransactionHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; }
        public MarketplaceListingDto Listing { get; set; }
        public UserDto Buyer { get; set; }
        public UserDto Seller { get; set; }
    }

    public class CreateMarketplaceTransactionDto
    {
        [Required]
        public string ListingId { get; set; }
        
        [Required]
        [Range(0.1, 10000)]
        public decimal Amount { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }
    }

    public class TransactionStatusUpdateDto
    {
        [Required]
        [RegularExpression("^(pending|completed|failed|cancelled)$")]
        public string Status { get; set; }
    }

    public class MarketplaceStatsDto
    {
        public decimal AverageBuyPrice { get; set; }
        public decimal AverageSellPrice { get; set; }
        public int ActiveBuyListings { get; set; }
        public int ActiveSellListings { get; set; }
        public decimal TotalVolumeLastDay { get; set; }
        public decimal TotalVolumeLastWeek { get; set; }
        public decimal TotalVolumeLastMonth { get; set; }
    }

    public class BlockchainOfferDto
    {
        public string OfferId { get; set; }
        public string OfferType { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public string Account { get; set; }
    }
}