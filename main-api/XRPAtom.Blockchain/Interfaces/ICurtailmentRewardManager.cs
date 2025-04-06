using XRPAtom.Core.Domain;

namespace XRPAtom.Blockchain.Interfaces
{
    /// <summary>
    /// Interface for managing reward distribution for curtailment events
    /// </summary>
    public interface ICurtailmentRewardManager
    {
        /// <summary>
        /// Creates the main funding pool for an event
        /// </summary>
        /// <param name="eventId">Event identifier</param>
        /// <param name="gridOperatorAddress">Grid operator's wallet address</param>
        /// <param name="totalAmount">Total amount of XRP for the reward pool</param>
        /// <returns>Details of the created reward pool</returns>
        Task<RewardPoolCreationResult> CreateEventFundingPool(string eventId, string gridOperatorAddress, decimal totalAmount);
        
        /// <summary>
        /// Allocates reward slots for participants based on their potential capacity
        /// </summary>
        /// <param name="eventId">Event identifier</param>
        /// <param name="participantIds">List of participant user IDs</param>
        /// <returns>True if allocation was successful</returns>
        Task<bool> AllocateParticipantRewards(string eventId, List<string> participantIds);
        
        /// <summary>
        /// Finalizes and distributes rewards after event completion and verification
        /// </summary>
        /// <param name="eventId">Event identifier</param>
        /// <returns>True if reward finalization was successful</returns>
        Task<bool> FinalizeRewards(string eventId);
        
        /// <summary>
        /// Processes a completed reward payment transaction
        /// </summary>
        /// <param name="payloadId">The XUMM payload ID for the payment</param>
        /// <param name="transactionHash">The transaction hash from the XRP Ledger</param>
        /// <returns>True if payment was processed successfully</returns>
        Task<bool> ProcessRewardPayment(string payloadId, string transactionHash);
    }
    
    /// <summary>
    /// Result of creating a reward pool
    /// </summary>
    public class RewardPoolCreationResult
    {
        /// <summary>
        /// Unique identifier for the reward pool
        /// </summary>
        public string PoolId { get; set; }
        
        /// <summary>
        /// Transaction hash on the XRP Ledger
        /// </summary>
        public string TransactionHash { get; set; }
        
        /// <summary>
        /// Total amount in the reward pool
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}