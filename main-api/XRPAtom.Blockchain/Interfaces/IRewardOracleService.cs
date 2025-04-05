namespace XRPAtom.Blockchain.Interfaces
{
    /// <summary>
    /// Defines the contract for a reward oracle service in the XRP Atom ecosystem
    /// Manages the verification and distribution of rewards for curtailment events
    /// </summary>
    public interface IRewardOracleService
    {
        /// <summary>
        /// Creates a reward pool for a specific curtailment event
        /// </summary>
        /// <param name="gridOperatorAddress">The address of the grid operator creating the pool</param>
        /// <param name="totalAmount">Total amount of rewards in the pool</param>
        /// <param name="parameters">Detailed parameters governing the reward pool</param>
        /// <returns>Details of the created reward pool</returns>
        Task<RewardPoolCreationResult> CreateRewardPool(
            string gridOperatorAddress, 
            decimal totalAmount, 
            RewardPoolParameters parameters);

        /// <summary>
        /// Verifies a user's participation in a curtailment event
        /// Performs comprehensive eligibility checks and generates verification proof
        /// </summary>
        /// <param name="eventId">Unique identifier of the curtailment event</param>
        /// <param name="userId">Identifier of the user being verified</param>
        /// <returns>Detailed verification result including eligibility and potential reward</returns>
        Task<RewardVerificationResult> VerifyParticipation(
            string eventId, 
            string userId);

        /// <summary>
        /// Allows a verified participant to claim their reward
        /// Performs final verification and initiates reward transfer
        /// </summary>
        /// <param name="eventId">Unique identifier of the curtailment event</param>
        /// <param name="userId">Identifier of the user claiming the reward</param>
        /// <returns>Result of the reward claim process</returns>
        Task<RewardClaimResult> ClaimReward(
            string eventId, 
            string userId);
    }

    /// <summary>
    /// Parameters defining the characteristics of a reward pool
    /// </summary>
    public class RewardPoolParameters
    {
        /// <summary>
        /// Type of event (e.g., Curtailment)
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Minimum energy saving threshold to be eligible for rewards
        /// </summary>
        public decimal MinimumEnergyThreshold { get; set; }

        /// <summary>
        /// Maximum number of participants eligible for rewards
        /// </summary>
        public int MaxEligibleParticipants { get; set; }

        /// <summary>
        /// Start date from which rewards become claimable
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Expiration date after which rewards cannot be claimed
        /// </summary>
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// Additional custom verification criteria
        /// </summary>
        public Dictionary<string, object> AdditionalCriteria { get; set; }
    }

    /// <summary>
    /// Represents the result of creating a reward pool
    /// </summary>
    public class RewardPoolCreationResult
    {
        /// <summary>
        /// Unique identifier for the created reward pool
        /// </summary>
        public string PoolId { get; set; }

        /// <summary>
        /// Transaction hash of the pool creation on the XRP Ledger
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// Total amount of rewards in the pool
        /// </summary>
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Represents the result of participation verification
    /// </summary>
    public class RewardVerificationResult
    {
        /// <summary>
        /// User identifier
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Event identifier
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Indicates whether the participation is verified
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Calculated reward amount
        /// </summary>
        public decimal CalculatedReward { get; set; }

        /// <summary>
        /// Cryptographic proof of participation
        /// </summary>
        public string VerificationProof { get; set; }

        /// <summary>
        /// List of verification errors (if any)
        /// </summary>
        public List<string> Errors { get; set; }
    }

    /// <summary>
    /// Represents the result of a reward claim attempt
    /// </summary>
    public class RewardClaimResult
    {
        /// <summary>
        /// Indicates whether the reward claim was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Amount of reward claimed
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Transaction hash of the reward transfer
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// List of errors if the claim was unsuccessful
        /// </summary>
        public List<string> Errors { get; set; }
    }
}