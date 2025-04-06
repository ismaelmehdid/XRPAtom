using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;
using XUMM.NET.SDK.Clients.Interfaces;
using XUMM.NET.SDK.Models.Payload;

namespace XRPAtom.Blockchain.Services
{
    /// <summary>
    /// Manages the distribution of rewards for curtailment events
    /// </summary>
    public class CurtailmentRewardManager : ICurtailmentRewardManager
    {
        private readonly IXummPayloadClient _xummPayloadClient;
        private readonly IEscrowService _escrowService;
        private readonly ICurtailmentEventService _curtailmentService;
        private readonly IXRPLedgerService _xrplService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CurtailmentRewardManager> _logger;

        // Configuration parameters
        private readonly string _platformReserveAddress;

        public CurtailmentRewardManager(
            IXummPayloadClient xummPayloadClient,
            IEscrowService escrowService,
            ICurtailmentEventService curtailmentService,
            IXRPLedgerService xrplService,
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            ILogger<CurtailmentRewardManager> logger)
        {
            _xummPayloadClient = xummPayloadClient;
            _escrowService = escrowService;
            _curtailmentService = curtailmentService;
            _xrplService = xrplService;
            _dbContext = dbContext;
            _logger = logger;
            
            // In production, this would come from configuration
            _platformReserveAddress = configuration["XRPLedger:PlatformReserveAddress"] ?? "rXRPAtomReserveAddressHere";
        }

        /// <summary>
        /// Creates the main funding pool for an event
        /// </summary>
        public async Task<RewardPoolCreationResult> CreateEventFundingPool(
            string eventId, string gridOperatorAddress, decimal totalAmount)
        {
            try
            {
                _logger.LogInformation("Creating funding pool for event {EventId} with amount {Amount} XRP", 
                    eventId, totalAmount);
                
                // Step 1: Create a custom reserve transaction to lock the total amount
                // This could be an escrow or a simple payment to the platform reserve
                var event_ = await _curtailmentService.GetEventByIdAsync(eventId);
                if (event_ == null)
                {
                    throw new ArgumentException($"Event {eventId} not found");
                }
                
                // Calculate release time (3 days after event ends)
                var releaseDate = DateTime.Parse(event_.EndTime.ToString()).AddDays(3);
                
                // Create the main escrow with the total pool amount
                var escrowResult = await _escrowService.CreateMainEventEscrow(
                    eventId,
                    gridOperatorAddress,
                    totalAmount,
                    releaseDate);
                    
                // Store escrow reference in the event
                await _curtailmentService.SetBlockchainReferenceAsync(eventId, escrowResult.EscrowId);
                
                return new RewardPoolCreationResult
                {
                    PoolId = escrowResult.EscrowId,
                    TransactionHash = "pending_signature", // Will be updated when signed
                    TotalAmount = totalAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating funding pool for event {EventId}", eventId);
                throw;
            }
        }
        
        /// <summary>
        /// Allocates reward slots for participants 
        /// </summary>
        public async Task<bool> AllocateParticipantRewards(string eventId, List<string> participantIds)
        {
            try
            {
                _logger.LogInformation("Allocating rewards for {Count} participants in event {EventId}", 
                    participantIds.Count, eventId);
                
                var event_ = await _curtailmentService.GetEventByIdAsync(eventId);
                if (event_ == null)
                {
                    throw new ArgumentException($"Event {eventId} not found");
                }
                
                // Find the main escrow to get grid operator information
                var mainEscrow = await _dbContext.EscrowDetails
                    .FirstOrDefaultAsync(e => e.EventId == eventId && e.EscrowType == "MainEvent");
                    
                if (mainEscrow == null)
                {
                    throw new InvalidOperationException($"Main escrow for event {eventId} not found");
                }
                
                // For each participant, allocate their potential reward
                foreach (var participantId in participantIds)
                {
                    // Get the participant's information
                    var participant = await _dbContext.Users
                        .Include(u => u.Wallet)
                        .FirstOrDefaultAsync(u => u.Id == participantId);
                        
                    if (participant?.Wallet == null)
                    {
                        _logger.LogWarning("Participant {ParticipantId} has no wallet, skipping allocation", participantId);
                        continue;
                    }
                    
                    // Check if allocation already exists
                    var existingAllocation = await _dbContext.RewardAllocations
                        .FirstOrDefaultAsync(ra => ra.EventId == eventId && ra.ParticipantId == participantId);
                        
                    if (existingAllocation != null)
                    {
                        _logger.LogInformation("Allocation already exists for participant {ParticipantId}", participantId);
                        continue;
                    }
                    
                    // Get their devices to calculate potential reward
                    var deviceCapacity = await _dbContext.Devices
                        .Where(d => d.UserId == participantId && d.Enrolled)
                        .SumAsync(d => d.EnergyCapacity);
                        
                    // Calculate potential reward based on capacity and reward per kWh
                    decimal potentialReward = (decimal)deviceCapacity * event_.RewardPerKwh;
                    
                    // Create a record for this allocation
                    var rewardAllocation = new RewardAllocation
                    {
                        EventId = eventId,
                        ParticipantId = participantId,
                        PotentialAmount = potentialReward,
                        ActualAmount = 0, // Will be filled when performance is verified
                        Status = "Allocated",
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.RewardAllocations.Add(rewardAllocation);
                }
                
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating rewards for event {EventId}", eventId);
                throw;
            }
        }
        
        /// <summary>
        /// Finalize and distribute rewards after event completion and verification
        /// </summary>
        public async Task<bool> FinalizeRewards(string eventId)
        {
            try
            {
                _logger.LogInformation("Finalizing rewards for event {EventId}", eventId);
                
                var event_ = await _curtailmentService.GetEventByIdAsync(eventId);
                if (event_ == null || event_.Status != EventStatus.Completed)
                {
                    throw new ArgumentException($"Event {eventId} not found or not completed");
                }
                
                // Get all participants with verified performance
                var participants = await _dbContext.EventParticipations
                    .Include(p => p.User)
                        .ThenInclude(u => u.Wallet)
                    .Where(p => p.EventId == eventId && p.Status == ParticipationStatus.Verified)
                    .ToListAsync();
                    
                if (!participants.Any())
                {
                    _logger.LogWarning("No verified participants found for event {EventId}", eventId);
                    return false;
                }
                
                // Get the main escrow
                var mainEscrow = await _dbContext.EscrowDetails
                    .FirstOrDefaultAsync(e => e.EventId == eventId && e.EscrowType == "MainEvent");
                    
                if (mainEscrow == null || mainEscrow.Status != "Active")
                {
                    throw new InvalidOperationException($"Active main escrow for event {eventId} not found");
                }
                
                // For each participant, create a payment transaction
                foreach (var participant in participants)
                {
                    if (participant.User?.Wallet == null)
                    {
                        _logger.LogWarning("Participant {ParticipantId} has no wallet, skipping payment", participant.UserId);
                        continue;
                    }
                    
                    // Calculate reward amount based on energy saved
                    decimal rewardAmount = participant.EnergySaved * event_.RewardPerKwh;
                    
                    if (rewardAmount <= 0)
                    {
                        _logger.LogInformation("No reward for participant {ParticipantId} with energy saved {EnergySaved}kWh",
                            participant.UserId, participant.EnergySaved);
                        continue;
                    }
                    
                    // Check if a payment already exists
                    var existingPayment = await _dbContext.RewardPayments
                        .FirstOrDefaultAsync(rp => rp.EventId == eventId && rp.ParticipantId == participant.UserId);
                        
                    if (existingPayment != null)
                    {
                        _logger.LogInformation("Payment already created for participant {ParticipantId}", participant.UserId);
                        continue;
                    }
                    
                    // Create a Payment transaction
                    var paymentJson = $@"{{
                        ""TransactionType"": ""Payment"",
                        ""Account"": ""{_platformReserveAddress}"",
                        ""Destination"": ""{participant.User.Wallet.Address}"",
                        ""Amount"": ""{ConvertXrpToDrops(rewardAmount)}"",
                        ""Memos"": [
                            {{
                                ""Memo"": {{
                                    ""MemoData"": ""{StringToHex($"XRPAtom Reward: Event {eventId}")}"",
                                    ""MemoType"": ""{StringToHex("text/plain")}""
                                }}
                            }}
                        ]
                    }}";
                    
                    var paymentPayload = new XummPostJsonPayload(paymentJson);
                    paymentPayload.CustomMeta = new XummPayloadCustomMeta
                    {
                        Identifier = $"reward_payment_{eventId}_{participant.UserId}",
                        Instruction = $"Payment of {rewardAmount} XRP for energy savings of {participant.EnergySaved} kWh"
                    };
                    
                    // This would typically be signed by an automated system or administrator
                    // For demonstration, we'll create a payload that can be signed manually
                    var paymentResponse = await _xummPayloadClient.CreateAsync(paymentPayload);
                    
                    // Record the payment
                    var rewardPayment = new RewardPayment
                    {
                        EventId = eventId,
                        ParticipantId = participant.UserId,
                        Amount = rewardAmount,
                        PayloadId = paymentResponse.Uuid,
                        Status = "PendingSignature",
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.RewardPayments.Add(rewardPayment);
                    
                    // Update the reward allocation if it exists
                    var allocation = await _dbContext.RewardAllocations
                        .FirstOrDefaultAsync(ra => ra.EventId == eventId && ra.ParticipantId == participant.UserId);
                        
                    if (allocation != null)
                    {
                        allocation.ActualAmount = rewardAmount;
                        allocation.Status = "Verified";
                        allocation.VerifiedAt = DateTime.UtcNow;
                    }
                }
                
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing rewards for event {EventId}", eventId);
                throw;
            }
        }
        
        /// <summary>
        /// Process a signed payment and update the participant's reward status
        /// </summary>
        public async Task<bool> ProcessRewardPayment(string payloadId, string transactionHash)
        {
            try
            {
                var payment = await _dbContext.RewardPayments
                    .FirstOrDefaultAsync(p => p.PayloadId == payloadId);
                    
                if (payment == null)
                {
                    _logger.LogWarning("Payment with payload {PayloadId} not found", payloadId);
                    return false;
                }
                
                // Update the payment record
                payment.TransactionHash = transactionHash;
                payment.Status = "Completed";
                payment.CompletedAt = DateTime.UtcNow;
                
                // Find and update the participant record
                var participation = await _dbContext.EventParticipations
                    .FirstOrDefaultAsync(p => 
                        p.EventId == payment.EventId && 
                        p.UserId == payment.ParticipantId);
                        
                if (participation != null)
                {
                    participation.RewardClaimed = true;
                    participation.RewardClaimedAt = DateTime.UtcNow;
                    participation.RewardAmount = payment.Amount;
                    participation.RewardTransactionId = transactionHash;
                }
                
                // Update the user's wallet with total rewards
                var wallet = await _dbContext.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == payment.ParticipantId);
                    
                if (wallet != null)
                {
                    wallet.TotalRewardsClaimed += payment.Amount;
                    wallet.LastUpdated = DateTime.UtcNow;
                }
                
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Processed reward payment of {Amount} XRP for participant {ParticipantId}",
                    payment.Amount, payment.ParticipantId);
                    
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reward payment for payload {PayloadId}", payloadId);
                throw;
            }
        }
        
        #region Helper Methods
        
        /// <summary>
        /// Converts XRP amount to drops (1 XRP = 1,000,000 drops)
        /// </summary>
        private string ConvertXrpToDrops(decimal xrp)
        {
            return ((ulong)(xrp * 1000000)).ToString();
        }
        
        /// <summary>
        /// Converts a string to hexadecimal format for use in memos
        /// </summary>
        private string StringToHex(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
        
        #endregion
    }
}