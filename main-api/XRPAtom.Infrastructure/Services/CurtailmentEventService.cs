using Microsoft.EntityFrameworkCore;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.Infrastructure.Services
{
    public class CurtailmentEventService : ICurtailmentEventService
    {
        private readonly ApplicationDbContext _context;

        public CurtailmentEventService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CurtailmentEventDto> GetEventByIdAsync(string eventId)
        {
            var @event = await _context.CurtailmentEvents
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (@event == null)
            {
                return null;
            }

            return new CurtailmentEventDto
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                StartTime = @event.StartTime,
                EndTime = @event.EndTime,
                Duration = @event.Duration,
                Status = @event.Status,
                RewardPerKwh = @event.RewardPerKwh,
                TotalEnergySaved = @event.TotalEnergySaved,
                TotalRewardsPaid = @event.TotalRewardsPaid,
                CreatedBy = @event.CreatedBy,
                BlockchainReference = @event.BlockchainReference,
                VerificationProof = @event.VerificationProof,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt,
                ParticipantCount = @event.Participations?.Count ?? 0
            };
        }

        public async Task<IEnumerable<CurtailmentEventDto>> GetAllEventsAsync(int page = 1, int pageSize = 10)
        {
            var events = await _context.CurtailmentEvents
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return events.Select(MapEventToDto);
        }

        public async Task<IEnumerable<CurtailmentEventDto>> GetEventsByStatusAsync(EventStatus status, int page = 1, int pageSize = 10)
        {
            var events = await _context.CurtailmentEvents
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return events.Select(MapEventToDto);
        }

        public async Task<IEnumerable<CurtailmentEventDto>> GetUserEventsAsync(string userId, int page = 1, int pageSize = 10)
        {
            var events = await _context.EventParticipations
                .Where(ep => ep.UserId == userId)
                .Select(ep => ep.Event)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return events.Select(MapEventToDto);
        }

        public async Task<int> GetUserEventCountAsync(string userId)
        {
            return await _context.EventParticipations
                .CountAsync(ep => ep.UserId == userId);
        }

        public async Task<CurtailmentEventDto> CreateEventAsync(CreateCurtailmentEventDto createEventDto)
        {
            var newEvent = new CurtailmentEvent
            {
                Title = createEventDto.Title,
                Description = createEventDto.Description,
                StartTime = createEventDto.StartTime,
                EndTime = createEventDto.EndTime,
                RewardPerKwh = createEventDto.RewardPerKwh,
                CreatedBy = createEventDto.CreatedBy,
                Status = EventStatus.Upcoming
            };

            _context.CurtailmentEvents.Add(newEvent);
            await _context.SaveChangesAsync();

            return MapEventToDto(newEvent);
        }

        public async Task<CurtailmentEventDto> UpdateEventAsync(string eventId, UpdateCurtailmentEventDto updateEventDto)
        {
            var existingEvent = await _context.CurtailmentEvents.FindAsync(eventId);
            
            if (existingEvent == null)
            {
                return null;
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(updateEventDto.Title))
                existingEvent.Title = updateEventDto.Title;

            if (!string.IsNullOrWhiteSpace(updateEventDto.Description))
                existingEvent.Description = updateEventDto.Description;

            if (updateEventDto.StartTime.HasValue)
                existingEvent.StartTime = updateEventDto.StartTime.Value;

            if (updateEventDto.EndTime.HasValue)
                existingEvent.EndTime = updateEventDto.EndTime.Value;

            if (updateEventDto.RewardPerKwh.HasValue)
                existingEvent.RewardPerKwh = updateEventDto.RewardPerKwh.Value;

            existingEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapEventToDto(existingEvent);
        }

        public async Task<bool> DeleteEventAsync(string eventId)
        {
            var @event = await _context.CurtailmentEvents.FindAsync(eventId);
            
            if (@event == null)
            {
                return false;
            }

            _context.CurtailmentEvents.Remove(@event);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEventStatusAsync(string eventId, EventStatus status)
        {
            var @event = await _context.CurtailmentEvents.FindAsync(eventId);
            
            if (@event == null)
            {
                return false;
            }

            @event.Status = status;
            @event.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegisterUserForEventAsync(string eventId, string userId, string walletAddress)
        {
            var @event = await _context.CurtailmentEvents.FindAsync(eventId);
            var user = await _context.Users.FindAsync(userId);

            if (@event == null || user == null)
            {
                return false;
            }

            var participation = new EventParticipation
            {
                EventId = eventId,
                UserId = userId,
                WalletAddress = walletAddress,
                Status = ParticipationStatus.Registered,
                RegisteredAt = DateTime.UtcNow
            };

            _context.EventParticipations.Add(participation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserParticipant(string eventId, string userId)
        {
            return await _context.EventParticipations
                .AnyAsync(ep => ep.EventId == eventId && ep.UserId == userId);
        }

        public async Task<EventParticipationDto> GetUserParticipation(string eventId, string userId)
        {
            var participation = await _context.EventParticipations
                .Include(ep => ep.Event)
                .Include(ep => ep.User)
                .FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);

            if (participation == null)
            {
                return null;
            }

            return new EventParticipationDto
            {
                EventId = participation.EventId,
                UserId = participation.UserId,
                WalletAddress = participation.WalletAddress,
                Status = participation.Status,
                EnergySaved = participation.EnergySaved,
                RewardAmount = participation.RewardAmount,
                RewardClaimed = participation.RewardClaimed,
                RegisteredAt = participation.RegisteredAt,
                VerifiedAt = participation.VerifiedAt,
                RewardClaimedAt = participation.RewardClaimedAt,
                User = new UserDto
                {
                    Id = participation.User.Id,
                    Name = participation.User.Name,
                    Email = participation.User.Email
                }
            };
        }

        public async Task<bool> RecordEnergySavedAsync(string eventId, string userId, decimal energySaved)
        {
            var participation = await _context.EventParticipations
                .FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);

            if (participation == null)
            {
                return false;
            }

            participation.EnergySaved = energySaved;
            participation.Status = ParticipationStatus.Completed;
            participation.VerifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private CurtailmentEventDto MapEventToDto(CurtailmentEvent @event)
        {
            return new CurtailmentEventDto
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                StartTime = @event.StartTime,
                EndTime = @event.EndTime,
                Duration = @event.Duration,
                Status = @event.Status,
                RewardPerKwh = @event.RewardPerKwh,
                TotalEnergySaved = @event.TotalEnergySaved,
                TotalRewardsPaid = @event.TotalRewardsPaid,
                CreatedBy = @event.CreatedBy,
                BlockchainReference = @event.BlockchainReference,
                VerificationProof = @event.VerificationProof,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt,
                ParticipantCount = @event.Participations?.Count ?? 0
            };
        }

        // Implement additional methods from the interface...
        public async Task<bool> VerifyParticipationAsync(string eventId, string userId)
        {
            var participation = await _context.EventParticipations
                .FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);

            if (participation == null)
            {
                return false;
            }

            participation.Status = ParticipationStatus.Verified;
            participation.VerifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkRewardClaimed(string eventId, string userId, decimal rewardAmount)
        {
            var participation = await _context.EventParticipations
                .FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);

            if (participation == null)
            {
                return false;
            }

            participation.RewardClaimed = true;
            participation.RewardAmount = rewardAmount;
            participation.RewardClaimedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<EventParticipationDto>> GetEventParticipantsAsync(string eventId, int page = 1, int pageSize = 25)
        {
            var participants = await _context.EventParticipations
                .Include(ep => ep.User)
                .Where(ep => ep.EventId == eventId)
                .OrderBy(ep => ep.RegisteredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return participants.Select(p => new EventParticipationDto
            {
                EventId = p.EventId,
                UserId = p.UserId,
                WalletAddress = p.WalletAddress,
                Status = p.Status,
                EnergySaved = p.EnergySaved,
                RewardAmount = p.RewardAmount,
                RewardClaimed = p.RewardClaimed,
                RegisteredAt = p.RegisteredAt,
                User = new UserDto
                {
                    Id = p.User.Id,
                    Name = p.User.Name,
                    Email = p.User.Email
                }
            });
        }

        public async Task<decimal> CalculateTotalEnergySavedAsync(string eventId)
        {
            return await _context.EventParticipations
                .Where(ep => ep.EventId == eventId)
                .SumAsync(ep => ep.EnergySaved);
        }

        public async Task<decimal> CalculateTotalRewardsForEventAsync(string eventId)
        {
            return await _context.EventParticipations
                .Where(ep => ep.EventId == eventId && ep.RewardClaimed)
                .SumAsync(ep => ep.RewardAmount);
        }

        public async Task<bool> SetBlockchainReferenceAsync(string eventId, string blockchainReference)
        {
            var @event = await _context.CurtailmentEvents.FindAsync(eventId);
            
            if (@event == null)
            {
                return false;
            }

            @event.BlockchainReference = blockchainReference;
            @event.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CurtailmentEventDto>> GetUpcomingEventsAsync(int hours, int page = 1, int pageSize = 10)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(hours);

            var events = await _context.CurtailmentEvents
                .Where(e => e.StartTime <= cutoffTime && e.Status == EventStatus.Upcoming)
                .OrderBy(e => e.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return events.Select(MapEventToDto);
        }

        public async Task<bool> StartEventAsync(string eventId)
        {
            var @event = await _context.CurtailmentEvents.FindAsync(eventId);
            
            if (@event == null || @event.Status != EventStatus.Upcoming)
            {
                return false;
            }

            @event.Status = EventStatus.Active;
            @event.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteEventAsync(string eventId)
        {
            var @event = await _context.CurtailmentEvents.FindAsync(eventId);
            
            if (@event == null || @event.Status != EventStatus.Active)
            {
                return false;
            }

            @event.Status = EventStatus.Completed;
            @event.UpdatedAt = DateTime.UtcNow;

            // Calculate total energy saved
            @event.TotalEnergySaved = await CalculateTotalEnergySavedAsync(eventId);
            @event.TotalRewardsPaid = await CalculateTotalRewardsForEventAsync(eventId);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}