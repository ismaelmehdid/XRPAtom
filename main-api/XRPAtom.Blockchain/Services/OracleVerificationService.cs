using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.Interfaces;
using XRPAtom.Blockchain.Models;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;

namespace XRPAtom.Blockchain.Services
{
    public class XRPLRewardOracleService : IRewardOracleService
    {
        private readonly IXRPLedgerService _xrplService;
        private readonly IXRPLTransactionService _transactionService;
        private readonly ICurtailmentEventService _curtailmentService;
        private readonly IUserWalletService _walletService;
        private readonly ILogger<XRPLRewardOracleService> _logger;

        public XRPLRewardOracleService(
            IXRPLedgerService xrplService,
            IXRPLTransactionService transactionService,
            ICurtailmentEventService curtailmentService,
            IUserWalletService walletService,
            ILogger<XRPLRewardOracleService> logger)
        {
            _xrplService = xrplService;
            _transactionService = transactionService;
            _curtailmentService = curtailmentService;
            _walletService = walletService;
            _logger = logger;
        }

        public async Task<RewardPoolCreationResult> CreateRewardPool(
            string gridOperatorAddress, 
            decimal totalAmount, 
            RewardPoolParameters parameters)
        {
            try 
            {
                // Prepare transaction request for escrow
                var transactionRequest = new TransactionPrepareRequest
                {
                    TransactionType = "Escrow",
                    SourceAddress = gridOperatorAddress,
                    DestinationAddress = GenerateRewardPoolAddress(),
                    Amount = totalAmount,
                    // Use flags or memo to encode additional parameters
                    Memo = JsonSerializer.Serialize(parameters)
                };

                // Prepare the transaction
                var prepareResponse = await _transactionService.PrepareTransaction(transactionRequest);

                // Return result matching the interface
                return new RewardPoolCreationResult
                {
                    PoolId = Guid.NewGuid().ToString(), // Generate a unique pool ID
                    TransactionHash = prepareResponse.PreparedTransaction, // Use prepared transaction as reference
                    TotalAmount = totalAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reward pool");
                throw;
            }
        }

        public async Task<RewardVerificationResult> VerifyParticipation(
            string eventId, 
            string userId)
        {
            try 
            {
                var participation = await _curtailmentService.GetUserParticipation(eventId, userId);
                
                // Generate verification proof
                var proof = await CreateVerificationProof(eventId, userId);

                // Comprehensive verification
                var verificationResult = new RewardVerificationResult
                {
                    UserId = userId,
                    EventId = eventId,
                    IsVerified = false,
                    Errors = new List<string>()
                };

                // Verification logic
                if (participation.Status != ParticipationStatus.Verified)
                {
                    verificationResult.Errors.Add("Participation not verified");
                    return verificationResult;
                }

                if (participation.EnergySaved < 1.0m)
                {
                    verificationResult.Errors.Add("Insufficient energy saved");
                    return verificationResult;
                }

                // Mark as verified
                verificationResult.IsVerified = true;
                verificationResult.CalculatedReward = CalculateReward(participation);
                verificationResult.VerificationProof = proof;

                return verificationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying participation");
                throw;
            }
        }

        public async Task<RewardClaimResult> ClaimReward(string eventId, string userId)
        {
            try 
            {
                // Verify participation first
                var verificationResult = await VerifyParticipation(eventId, userId);
                
                if (!verificationResult.IsVerified)
                {
                    return new RewardClaimResult 
                    {
                        Success = false,
                        Errors = verificationResult.Errors
                    };
                }

                // Get user's wallet
                var userWallet = await _walletService.GetWalletByUserIdAsync(userId);
                
                // Prepare reward transfer transaction
                var transactionRequest = new TransactionPrepareRequest
                {
                    TransactionType = "Payment",
                    SourceAddress = GenerateRewardPoolAddress(),
                    DestinationAddress = userWallet.Address,
                    Amount = verificationResult.CalculatedReward,
                    Memo = verificationResult.VerificationProof
                };

                // Prepare and submit the transaction
                var prepareResponse = await _transactionService.PrepareTransaction(transactionRequest);

                return new RewardClaimResult 
                {
                    Success = true,
                    Amount = verificationResult.CalculatedReward,
                    TransactionHash = prepareResponse.PreparedTransaction
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error claiming reward");
                throw;
            }
        }

        // Helper methods
        private string GenerateRewardPoolAddress()
        {
            // Generate a deterministic reward pool address
            return "rRewardPoolXRPAtom" + Guid.NewGuid().ToString().Substring(0, 20);
        }

        private decimal CalculateReward(EventParticipationDto participation)
        {
            // Simple reward calculation based on energy saved
            // In a real-world scenario, this would be more complex
            return participation.EnergySaved * 0.1m; // 0.1 XRP per kWh saved
        }

        private async Task<string> CreateVerificationProof(string eventId, string userId)
        {
            // Create a cryptographic proof of participation
            var participation = await _curtailmentService.GetUserParticipation(eventId, userId);
            
            var proofData = new 
            {
                EventId = eventId,
                UserId = userId,
                EnergySaved = participation.EnergySaved,
                Timestamp = DateTime.UtcNow
            };

            // Use a secure hash to create the proof
            return ComputeSHA256Hash(JsonSerializer.Serialize(proofData));
        }

        private string ComputeSHA256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}