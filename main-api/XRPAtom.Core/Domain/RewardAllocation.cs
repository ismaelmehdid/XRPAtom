using System.ComponentModel.DataAnnotations;

namespace XRPAtom.Core.Domain
{
    /// <summary>
    /// Represents a potential reward allocation for a participant in a curtailment event
    /// </summary>
    public class RewardAllocation
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string EventId { get; set; }
        
        [Required]
        public string ParticipantId { get; set; }
        
        [Required]
        public decimal PotentialAmount { get; set; }
        
        public decimal ActualAmount { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } // Allocated, Verified, Cancelled
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? VerifiedAt { get; set; }
        
        // Navigation properties
        public virtual CurtailmentEvent Event { get; set; }
        
        public virtual User Participant { get; set; }
    }
}