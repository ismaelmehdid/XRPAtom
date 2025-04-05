using System.ComponentModel.DataAnnotations;

namespace XRPAtom.Core.Domain
{
    public class UserWallet
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Address { get; set; }
        
        [StringLength(150)]
        public string PublicKey { get; set; }
        
        public decimal Balance { get; set; }
        
        // Optional: Token balance for ATOM tokens
        public decimal AtomTokenBalance { get; set; }
        
        public decimal TotalRewardsClaimed { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastUpdated { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Optional: Track wallet association with devices
        public bool IsLinkedToDevices { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
    }
}