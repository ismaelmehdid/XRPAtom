using System.ComponentModel.DataAnnotations;

namespace XRPAtom.Core.Domain
{
    /// <summary>
    /// Represents a payment of rewards to a participant for a curtailment event
    /// </summary>
    public class RewardPayment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string EventId { get; set; }
        
        [Required]
        public string ParticipantId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [StringLength(200)]
        public string PayloadId { get; set; }
        
        [StringLength(200)]
        public string TransactionHash { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } // PendingSignature, Completed, Failed
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }
        
        // Navigation properties
        public virtual CurtailmentEvent Event { get; set; }
        
        public virtual User Participant { get; set; }
    }
}