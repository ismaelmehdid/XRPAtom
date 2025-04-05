using System.ComponentModel.DataAnnotations;

namespace XRPAtom.Core.Domain
{
    public class CurtailmentEvent
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        [Required]
        public int Duration => (int)(EndTime - StartTime).TotalMinutes; // Duration in minutes
        
        [Required]
        public EventStatus Status { get; set; } = EventStatus.Upcoming;
        
        [Required]
        public decimal RewardPerKwh { get; set; } // Reward in XRP per kWh curtailed
        
        public decimal TotalEnergySaved { get; set; } = 0; // Total kWh saved across all participants
        
        public decimal TotalRewardsPaid { get; set; } = 0; // Total XRP paid out
        
        [Required]
        public string CreatedBy { get; set; } // User ID who created the event
        
        [StringLength(200)]
        public string BlockchainReference { get; set; } // Reference to blockchain record
        
        [StringLength(500)]
        public string VerificationProof { get; set; } // Verification proof for the event
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<EventParticipation> Participations { get; set; } = new List<EventParticipation>();
    }
    
    public enum EventStatus
    {
        Upcoming = 0,
        Active = 1,
        Completed = 2,
        Cancelled = 3,
        Failed = 4
    }
}