using System.ComponentModel.DataAnnotations;

namespace XRPAtom.Core.Domain
{
    public class Transaction
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(100)]
        public string SourceAddress { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DestinationAddress { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [StringLength(10)]
        public string Currency { get; set; } = "XRP"; // Default to XRP
        
        [StringLength(100)]
        public string Issuer { get; set; } // Token issuer address if not XRP
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; } // Payment, TrustSet, OfferCreate, etc.
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } // submitted, pending, completed, failed
        
        [StringLength(200)]
        public string TransactionHash { get; set; } // XRPL transaction hash
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [StringLength(500)]
        public string Memo { get; set; } // Transaction memo
        
        public int RetryCount { get; set; } = 0;
        
        public uint? Sequence { get; set; } // XRPL sequence number
        
        public string? RelatedEntityId { get; set; } // ID of related entity (e.g., CurtailmentEvent)
        
        [StringLength(50)]
        public string? RelatedEntityType { get; set; } // Type of related entity
        
        [StringLength(4000)]
        public string? RawResponse { get; set; } // Full response from XRPL
    }
}