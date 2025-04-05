using System.ComponentModel.DataAnnotations;

namespace XRPAtom.Core.Domain
{
    public class MarketplaceListing
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Type { get; set; } // "buy" or "sell"
        
        [Required]
        public string Provider { get; set; } // User ID of the listing creator
        
        [Required]
        [StringLength(50)]
        public string ProviderType { get; set; } // "grid_operator", "energy_supplier", "residential", "commercial"
        
        [Required]
        [StringLength(200)]
        public string AvailabilityWindow { get; set; } // JSON representation of availability window
        
        [Required]
        public decimal PricePerKwh { get; set; } // Price in XRP per kWh
        
        [Required]
        public decimal MinKwh { get; set; } // Minimum energy in kWh
        
        [Required]
        public decimal MaxKwh { get; set; } // Maximum energy in kWh
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "active"; // "active", "inactive", "completed", "expired"
        
        [StringLength(200)]
        public string BlockchainReference { get; set; } // Reference to XRPL offer ID
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
        
        // Navigation properties
        public virtual User ProviderUser { get; set; }
    }

    public class MarketplaceTransaction
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string ListingId { get; set; }
        
        [Required]
        public string BuyerId { get; set; } // User ID of the buyer
        
        [Required]
        public string SellerId { get; set; } // User ID of the seller
        
        [Required]
        public decimal Amount { get; set; } // Amount of energy in kWh
        
        [Required]
        public decimal TotalPrice { get; set; } // Total price in XRP
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "pending"; // "pending", "completed", "failed", "cancelled"
        
        [StringLength(200)]
        public string TransactionHash { get; set; } // XRPL transaction hash
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // Navigation properties
        public virtual MarketplaceListing Listing { get; set; }
        
        public virtual User Buyer { get; set; }
        
        public virtual User Seller { get; set; }
    }
}