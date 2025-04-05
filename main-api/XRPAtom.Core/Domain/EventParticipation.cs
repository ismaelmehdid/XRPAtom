using System;
using System.ComponentModel.DataAnnotations;

namespace XRPAtom.Core.Domain
{
    public class EventParticipation
    {
        // Composite key: EventId + UserId
        [Required]
        public string EventId { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string WalletAddress { get; set; } // Participant's wallet address
        
        public ParticipationStatus Status { get; set; } = ParticipationStatus.Registered;
        
        public decimal EnergySaved { get; set; } = 0; // kWh curtailed during event
        
        public decimal RewardAmount { get; set; } = 0; // XRP reward amount
        
        public bool RewardClaimed { get; set; } = false; // Whether reward has been claimed
        
        [StringLength(200)]
        public string RewardTransactionId { get; set; } // Transaction ID for reward payment
        
        [StringLength(500)]
        public string VerificationData { get; set; } // Data for verifying participation
        
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? VerifiedAt { get; set; }
        
        public DateTime? RewardClaimedAt { get; set; }
        
        // Navigation properties
        public virtual CurtailmentEvent Event { get; set; }
        
        public virtual User User { get; set; }
    }
    
    public enum ParticipationStatus
    {
        Registered = 0, // User registered for event
        Participating = 1, // User is actively participating
        Completed = 2, // User completed participation
        Verified = 3, // Participation has been verified
        Failed = 4, // User failed to participate 
        Missed = 5 // User missed the event entirely
    }
}