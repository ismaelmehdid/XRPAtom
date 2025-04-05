using System.ComponentModel.DataAnnotations;

namespace XRPAtom.Core.DTOs
{
    public class UserWalletDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Address { get; set; }
        public string PublicKey { get; set; }
        public decimal Balance { get; set; }
        public decimal AtomTokenBalance { get; set; }
        public decimal TotalRewardsClaimed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
        public bool IsActive { get; set; }
    }

    public class WalletBalanceDto
    {
        public decimal Balance { get; set; }
        public decimal AtomTokenBalance { get; set; }
        public string Address { get; set; }
    }

    public class CreateWalletDto
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Address { get; set; }
        
        [StringLength(150)]
        public string PublicKey { get; set; }
    }

    public class WalletImportDto
    {
        [Required]
        [StringLength(50)]
        public string Address { get; set; }
    }

    public class WalletCreateResponseDto
    {
        public string Address { get; set; }
        public string PublicKey { get; set; }
        
        // Note: Secret is only returned during wallet generation and never stored
        public string Secret { get; set; }
    }

    public class WalletTransactionDto
    {
        public string Id { get; set; }
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string TransactionHash { get; set; }
        public DateTime Timestamp { get; set; }
        public string Memo { get; set; }
    }

    public class CreateTransactionDto
    {
        [Required]
        [StringLength(100)]
        public string DestinationAddress { get; set; }
        
        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [StringLength(10)]
        public string Currency { get; set; } = "XRP";
        
        [StringLength(500)]
        public string Memo { get; set; }
    }
}