using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using XRPAtom.Core.Domain;

namespace XRPAtom.Core.DTOs
{
    public class CurtailmentEventDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; } // In minutes
        public EventStatus Status { get; set; }
        public decimal RewardPerKwh { get; set; }
        public decimal TotalEnergySaved { get; set; }
        public decimal TotalRewardsPaid { get; set; }
        public string CreatedBy { get; set; }
        public string BlockchainReference { get; set; }
        public string VerificationProof { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ParticipantCount { get; set; }
        public bool UserIsParticipant { get; set; } // Indicates if the requesting user is a participant
    }

    public class CreateCurtailmentEventDto
    {
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
        [Range(0.01, 100)]
        public decimal RewardPerKwh { get; set; }
        
        [Required]
        public string CreatedBy { get; set; } // User ID who created the event
    }

    public class UpdateCurtailmentEventDto
    {
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public DateTime? StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        [Range(0.01, 100)]
        public decimal? RewardPerKwh { get; set; }
    }

    public class EventStatusUpdateDto
    {
        [Required]
        public EventStatus Status { get; set; }
    }

    public class EventParticipationDto
    {
        public string EventId { get; set; }
        public string UserId { get; set; }
        public string WalletAddress { get; set; }
        public ParticipationStatus Status { get; set; }
        public decimal EnergySaved { get; set; }
        public decimal RewardAmount { get; set; }
        public bool RewardClaimed { get; set; }
        public string RewardTransactionId { get; set; }
        public string VerificationData { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? RewardClaimedAt { get; set; }
        public UserDto User { get; set; }
    }

    public class EventRegistrationDto
    {
        [Required]
        public string EventId { get; set; }
        
        [Required]
        public string WalletAddress { get; set; }
    }

    public class EnergySavedRecordDto
    {
        [Required]
        [Range(0.01, 1000)]
        public decimal EnergySaved { get; set; }
        
        [StringLength(500)]
        public string VerificationData { get; set; }
    }

    public class EventSummaryDto
    {
        public int TotalEvents { get; set; }
        public int CompletedEvents { get; set; }
        public int ActiveEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public decimal TotalEnergySaved { get; set; }
        public decimal TotalRewardsPaid { get; set; }
        public int TotalParticipations { get; set; }
    }

    public class EventVerificationDto
    {
        [Required]
        public string EventId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string VerificationProof { get; set; }
        
        [Required]
        [Range(0.01, 100000)]
        public decimal TotalEnergySaved { get; set; }
    }
}