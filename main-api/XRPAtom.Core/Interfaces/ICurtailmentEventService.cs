using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;

namespace XRPAtom.Core.Interfaces
{
    public interface ICurtailmentEventService
    {
        Task<CurtailmentEventDto> GetEventByIdAsync(string eventId);
        
        Task<IEnumerable<CurtailmentEventDto>> GetAllEventsAsync(int page = 1, int pageSize = 10);
        
        Task<IEnumerable<CurtailmentEventDto>> GetEventsByStatusAsync(EventStatus status, int page = 1, int pageSize = 10);
        
        Task<IEnumerable<CurtailmentEventDto>> GetUserEventsAsync(string userId, int page = 1, int pageSize = 10);
        
        Task<int> GetUserEventCountAsync(string userId);
        
        Task<CurtailmentEventDto> CreateEventAsync(CreateCurtailmentEventDto createEventDto);
        
        Task<CurtailmentEventDto> UpdateEventAsync(string eventId, UpdateCurtailmentEventDto updateEventDto);
        
        Task<bool> DeleteEventAsync(string eventId);
        
        Task<bool> UpdateEventStatusAsync(string eventId, EventStatus status);
        
        Task<bool> RegisterUserForEventAsync(string eventId, string userId, string walletAddress);
        
        Task<bool> IsUserParticipant(string eventId, string userId);
        
        Task<EventParticipationDto> GetUserParticipation(string eventId, string userId);
        
        Task<bool> RecordEnergySavedAsync(string eventId, string userId, decimal energySaved);
        
        Task<bool> VerifyParticipationAsync(string eventId, string userId);
        
        Task<bool> MarkRewardClaimed(string eventId, string userId, decimal rewardAmount);
        
        Task<IEnumerable<EventParticipationDto>> GetEventParticipantsAsync(string eventId, int page = 1, int pageSize = 25);
        
        Task<decimal> CalculateTotalEnergySavedAsync(string eventId);
        
        Task<decimal> CalculateTotalRewardsForEventAsync(string eventId);
        
        Task<bool> SetBlockchainReferenceAsync(string eventId, string blockchainReference);
        
        Task<IEnumerable<CurtailmentEventDto>> GetUpcomingEventsAsync(int hours, int page = 1, int pageSize = 10);
        
        Task<bool> StartEventAsync(string eventId);
        
        Task<bool> CompleteEventAsync(string eventId);
    }
}