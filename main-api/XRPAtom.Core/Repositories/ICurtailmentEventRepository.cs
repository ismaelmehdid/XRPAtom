using XRPAtom.Core.Domain;

namespace XRPAtom.Core.Repositories
{
    /// <summary>
    /// Defines the contract for curtailment event database operations
    /// </summary>
    public interface ICurtailmentEventRepository
    {
        /// <summary>
        /// Retrieves a curtailment event by its unique identifier
        /// </summary>
        /// <param name="eventId">The unique event identifier</param>
        /// <returns>The curtailment event or null if not found</returns>
        Task<CurtailmentEvent> GetByIdAsync(string eventId);

        /// <summary>
        /// Retrieves all curtailment events with optional pagination
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">Number of events per page</param>
        /// <returns>A collection of curtailment events</returns>
        Task<IEnumerable<CurtailmentEvent>> GetAllAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// Retrieves curtailment events by their status
        /// </summary>
        /// <param name="status">The event status to filter by</param>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">Number of events per page</param>
        /// <returns>A collection of curtailment events with the specified status</returns>
        Task<IEnumerable<CurtailmentEvent>> GetByStatusAsync(EventStatus status, int page = 1, int pageSize = 10);

        /// <summary>
        /// Retrieves curtailment events for a specific user
        /// </summary>
        /// <param name="userId">The unique user identifier</param>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">Number of events per page</param>
        /// <returns>A collection of curtailment events the user is participating in</returns>
        Task<IEnumerable<CurtailmentEvent>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Creates a new curtailment event
        /// </summary>
        /// <param name="curtailmentEvent">The curtailment event to create</param>
        /// <returns>The created curtailment event</returns>
        Task<CurtailmentEvent> CreateAsync(CurtailmentEvent curtailmentEvent);

        /// <summary>
        /// Updates an existing curtailment event
        /// </summary>
        /// <param name="curtailmentEvent">The curtailment event with updated information</param>
        /// <returns>The updated curtailment event</returns>
        Task<CurtailmentEvent> UpdateAsync(CurtailmentEvent curtailmentEvent);

        /// <summary>
        /// Deletes a curtailment event
        /// </summary>
        /// <param name="eventId">The unique identifier of the event to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteAsync(string eventId);

        /// <summary>
        /// Updates the status of a curtailment event
        /// </summary>
        /// <param name="eventId">The unique identifier of the event</param>
        /// <param name="status">The new status</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateStatusAsync(string eventId, EventStatus status);

        /// <summary>
        /// Retrieves upcoming events within a specified time frame
        /// </summary>
        /// <param name="hours">Number of hours to look ahead</param>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">Number of events per page</param>
        /// <returns>A collection of upcoming events</returns>
        Task<IEnumerable<CurtailmentEvent>> GetUpcomingEventsAsync(int hours, int page = 1, int pageSize = 10);

        /// <summary>
        /// Counts the total number of events for a specific user
        /// </summary>
        /// <param name="userId">The unique user identifier</param>
        /// <returns>Total number of events for the user</returns>
        Task<int> GetUserEventCountAsync(string userId);

        /// <summary>
        /// Sets the blockchain reference for an event
        /// </summary>
        /// <param name="eventId">The unique event identifier</param>
        /// <param name="blockchainReference">The blockchain reference to set</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> SetBlockchainReferenceAsync(string eventId, string blockchainReference);
    }
}