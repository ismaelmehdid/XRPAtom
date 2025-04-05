using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XRPAtom.Core.Domain;
using XRPAtom.Core.Repositories;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.Infrastructure.Data.Repositories
{
    public class CurtailmentEventRepository : ICurtailmentEventRepository
    {
        private readonly ApplicationDbContext _context;

        public CurtailmentEventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CurtailmentEvent> GetByIdAsync(string eventId)
        {
            return await _context.CurtailmentEvents
                .Include(e => e.Participations)
                .FirstOrDefaultAsync(e => e.Id == eventId);
        }

        public async Task<IEnumerable<CurtailmentEvent>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.CurtailmentEvents
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<CurtailmentEvent>> GetByStatusAsync(EventStatus status, int page = 1, int pageSize = 10)
        {
            return await _context.CurtailmentEvents
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<CurtailmentEvent>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 10)
        {
            return await _context.EventParticipations
                .Where(ep => ep.UserId == userId)
                .Select(ep => ep.Event)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<CurtailmentEvent> CreateAsync(CurtailmentEvent curtailmentEvent)
        {
            // Ensure ID is generated if not provided
            if (string.IsNullOrEmpty(curtailmentEvent.Id))
            {
                curtailmentEvent.Id = Guid.NewGuid().ToString();
            }

            // Set creation timestamp
            curtailmentEvent.CreatedAt = DateTime.UtcNow;

            _context.CurtailmentEvents.Add(curtailmentEvent);
            await _context.SaveChangesAsync();
            return curtailmentEvent;
        }

        public async Task<CurtailmentEvent> UpdateAsync(CurtailmentEvent curtailmentEvent)
        {
            var existingEvent = await _context.CurtailmentEvents.FindAsync(curtailmentEvent.Id);
            
            if (existingEvent == null)
            {
                throw new InvalidOperationException("Event not found.");
            }

            // Update only mutable properties
            existingEvent.Title = curtailmentEvent.Title ?? existingEvent.Title;
            existingEvent.Description = curtailmentEvent.Description ?? existingEvent.Description;
            existingEvent.StartTime = curtailmentEvent.StartTime;
            existingEvent.EndTime = curtailmentEvent.EndTime;
            existingEvent.RewardPerKwh = curtailmentEvent.RewardPerKwh;
            existingEvent.Status = curtailmentEvent.Status;
            existingEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingEvent;
        }

        public async Task<bool> DeleteAsync(string eventId)
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

        public async Task<bool> UpdateStatusAsync(string eventId, EventStatus status)
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

        public async Task<IEnumerable<CurtailmentEvent>> GetUpcomingEventsAsync(int hours, int page = 1, int pageSize = 10)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(hours);

            return await _context.CurtailmentEvents
                .Where(e => e.StartTime <= cutoffTime && e.Status == EventStatus.Upcoming)
                .OrderBy(e => e.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUserEventCountAsync(string userId)
        {
            return await _context.EventParticipations
                .CountAsync(ep => ep.UserId == userId);
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
    }
}