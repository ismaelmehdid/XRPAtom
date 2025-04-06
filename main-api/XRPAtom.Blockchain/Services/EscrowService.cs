using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;
using XUMM.NET.SDK.Clients.Interfaces;
using XUMM.NET.SDK.Models.Payload;

namespace XRPAtom.Blockchain.Services
{
    public class EscrowService : IEscrowService
    {
        private readonly IXummPayloadClient _xummPayloadClient;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<EscrowService> _logger;
        private readonly string _platformReserveAddress;

        public EscrowService(
            IXummPayloadClient xummPayloadClient,
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            ILogger<EscrowService> logger)
        {
            _xummPayloadClient = xummPayloadClient;
            _dbContext = dbContext;
            _logger = logger;
            
            // Get platform reserve address from configuration or use a default
            _platformReserveAddress = configuration["XRPLedger:PlatformReserveAddress"] 
                ?? configuration["Xrpl:PlatformReserveAddress"] 
                ?? "rRewardPoolXRPAtom";
        }

        /// <summary>
        /// Creates a main escrow for the curtailment event with the total reward pool
        /// </summary>
        public async Task<EscrowCreationResultDto> CreateMainEventEscrow(
            string eventId, 
            string sourceAddress, 
            decimal totalAmount,
            DateTime releaseDate)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(eventId))
                    throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
                    
                if (string.IsNullOrWhiteSpace(sourceAddress))
                    throw new ArgumentException("Source address cannot be empty", nameof(sourceAddress));
                    
                if (totalAmount <= 0)
                    throw new ArgumentException("Total amount must be greater than zero", nameof(totalAmount));
                    
                if (releaseDate <= DateTime.UtcNow)
                    throw new ArgumentException("Release date must be in the future", nameof(releaseDate));
                
                // Generate cryptographic condition for this event
                var (condition, fulfillment) = GenerateCryptoCondition();
                
                // Convert to Ripple time (seconds since Ripple epoch)
                uint finishAfter = ConvertToRippleTime(releaseDate);
                
                // Calculate cancel_after (1 day after finish_after)
                uint cancelAfter = finishAfter + 86400;

                // Create EscrowCreate payload according to XRPL specs
                var txJson = JsonSerializer.Serialize(new
                {
                    TransactionType = "EscrowCreate",
                    Account = sourceAddress,
                    Amount = ConvertXrpToDrops(totalAmount),
                    Destination = _platformReserveAddress,
                    FinishAfter = finishAfter,
                    CancelAfter = cancelAfter,
                });

                var payload = new XummPostJsonPayload(txJson);

                // Send to XUMM for signing
                var response = await _xummPayloadClient.CreateAsync(payload);
                
                if (response == null)
                {
                    throw new InvalidOperationException("Failed to create XUMM payload");
                }
                
                // Store escrow details for later use
                var escrowDetails = new EscrowDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    EventId = eventId,
                    EscrowType = "MainEvent",
                    SourceAddress = sourceAddress,
                    DestinationAddress = _platformReserveAddress,
                    Amount = totalAmount,
                    Condition = condition,
                    Fulfillment = fulfillment,
                    FinishAfter = finishAfter,
                    XummPayloadId = response.Uuid,
                    CreatedAt = DateTime.UtcNow,
                    ParticipantId = "",
                    FinishPayloadId = "",
                    // Sequence and hash will be populated after transaction is signed
                    Status = "Pending",
                    CancelPayloadId = "",
                    TransactionHash = "",
                    OfferSequence = "",
                };
                
                _dbContext.EscrowDetails.Add(escrowDetails);
                await _dbContext.SaveChangesAsync();
                
                return new EscrowCreationResultDto
                {
                    Success = true,
                    EscrowId = escrowDetails.Id,
                    Amount = totalAmount,
                    XummPayloadId = response.Uuid,
                    QrCodeUrl = response.Refs.QrPng,
                    DeepLink = response.Next.Always,
                    Condition = condition,
                    Fulfillment = fulfillment
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating main event escrow for event {EventId}", eventId);
                throw;
            }
        }

        /// <summary>
        /// Creates an individual escrow for a participant in the curtailment event
        /// </summary>
        public async Task<EscrowCreationResultDto> CreateParticipantEscrow(
            string eventId,
            string participantId,
            string sourceAddress,
            string destinationAddress,
            decimal amount,
            DateTime releaseDate)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(eventId))
                    throw new ArgumentException("Event ID cannot be empty", nameof(eventId));
                    
                if (string.IsNullOrWhiteSpace(participantId))
                    throw new ArgumentException("Participant ID cannot be empty", nameof(participantId));
                    
                if (string.IsNullOrWhiteSpace(sourceAddress))
                    throw new ArgumentException("Source address cannot be empty", nameof(sourceAddress));
                    
                if (string.IsNullOrWhiteSpace(destinationAddress))
                    throw new ArgumentException("Destination address cannot be empty", nameof(destinationAddress));
                    
                if (amount <= 0)
                    throw new ArgumentException("Amount must be greater than zero", nameof(amount));
                    
                if (releaseDate <= DateTime.UtcNow)
                    throw new ArgumentException("Release date must be in the future", nameof(releaseDate));
                
                // Generate unique cryptographic condition for this participant
                var (condition, fulfillment) = GenerateCryptoCondition();
                
                // Convert to Ripple time
                uint finishAfter = ConvertToRippleTime(releaseDate);
                
                // Calculate cancel_after (1 day after finish_after)
                uint cancelAfter = finishAfter + 86400;

                // Create EscrowCreate payload according to XRPL specs
                var txJson = JsonSerializer.Serialize(new
                {
                    TransactionType = "EscrowCreate",
                    Account = _platformReserveAddress,
                    Amount = ConvertXrpToDrops(amount),
                    Destination = sourceAddress,
                    FinishAfter = finishAfter,
                    CancelAfter = cancelAfter,
                });

                var payload = new XummPostJsonPayload(txJson);

                // Send to XUMM for signing
                var response = await _xummPayloadClient.CreateAsync(payload);
                
                if (response == null)
                {
                    throw new InvalidOperationException("Failed to create XUMM payload");
                }
                
                // Store escrow details for later use
                var escrowDetails = new EscrowDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    EventId = eventId,
                    EscrowType = "Participant",
                    SourceAddress = sourceAddress,
                    DestinationAddress = _platformReserveAddress,
                    Amount = amount,
                    Condition = condition,
                    Fulfillment = fulfillment,
                    FinishAfter = finishAfter,
                    XummPayloadId = response.Uuid,
                    CreatedAt = DateTime.UtcNow,
                    ParticipantId = "",
                    FinishPayloadId = "",
                    // Sequence and hash will be populated after transaction is signed
                    Status = "Pending",
                    CancelPayloadId = "",
                    TransactionHash = "",
                    OfferSequence = "",
                };
                
                _dbContext.EscrowDetails.Add(escrowDetails);
                await _dbContext.SaveChangesAsync();
                
                return new EscrowCreationResultDto
                {
                    Success = true,
                    EscrowId = escrowDetails.Id,
                    Amount = amount,
                    XummPayloadId = response.Uuid,
                    QrCodeUrl = response.Refs.QrPng,
                    DeepLink = response.Next.Always,
                    Condition = condition,
                    Fulfillment = fulfillment
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating participant escrow for event {EventId}, participant {ParticipantId}", 
                    eventId, participantId);
                throw;
            }
        }

        /// <summary>
        /// Finishes an escrow, releasing the funds to the destination
        /// </summary>
        public async Task<EscrowFinishResultDto> FinishEscrow(string escrowId, string signerAddress)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(escrowId))
                    throw new ArgumentException("Escrow ID cannot be empty", nameof(escrowId));
                    
                if (string.IsNullOrWhiteSpace(signerAddress))
                    throw new ArgumentException("Signer address cannot be empty", nameof(signerAddress));
                
                // Get escrow details from database
                var escrowDetails = await _dbContext.EscrowDetails.FindAsync(escrowId);
                
                if (escrowDetails == null)
                {
                    throw new ArgumentException($"Escrow with ID {escrowId} not found");
                }
                
                if (escrowDetails.Status != "Active")
                {
                    throw new InvalidOperationException($"Escrow is not in active state. Current state: {escrowDetails.Status}");
                }
                
                if (string.IsNullOrEmpty(escrowDetails.OfferSequence))
                {
                    throw new InvalidOperationException("Escrow sequence is not available");
                }

                // Create EscrowFinish transaction according to XRPL specs
                var txJson = new
                {
                    TransactionType = "EscrowFinish",
                    Account = signerAddress,
                    Owner = escrowDetails.SourceAddress,
                    OfferSequence = uint.Parse(escrowDetails.OfferSequence),
                    Condition = escrowDetails.Condition,
                    Fulfillment = escrowDetails.Fulfillment,
                    Identifier = $"finish_escrow_{escrowId}"
                };

                var payloadJson = JsonSerializer.Serialize(txJson);
                var payload = new XummPostJsonPayload(payloadJson);
                
                // Send to XUMM for signing
                var response = await _xummPayloadClient.CreateAsync(payload);
                
                if (response == null)
                {
                    throw new InvalidOperationException("Failed to create XUMM payload");
                }
                
                // Update escrow status
                escrowDetails.FinishPayloadId = response.Uuid;
                escrowDetails.Status = "FinishPending";
                escrowDetails.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                
                return new EscrowFinishResultDto
                {
                    Success = true,
                    EscrowId = escrowId,
                    XummPayloadId = response.Uuid,
                    QrCodeUrl = response.Refs.QrPng,
                    DeepLink = response.Next.Always
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finishing escrow {EscrowId}", escrowId);
                throw;
            }
        }

        /// <summary>
        /// Cancels an escrow, returning the funds to the source
        /// </summary>
        public async Task<EscrowCancelResultDto> CancelEscrow(string escrowId, string signerAddress)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(escrowId))
                    throw new ArgumentException("Escrow ID cannot be empty", nameof(escrowId));
                    
                if (string.IsNullOrWhiteSpace(signerAddress))
                    throw new ArgumentException("Signer address cannot be empty", nameof(signerAddress));
                
                // Get escrow details from database
                var escrowDetails = await _dbContext.EscrowDetails.FindAsync(escrowId);
                
                if (escrowDetails == null)
                {
                    throw new ArgumentException($"Escrow with ID {escrowId} not found");
                }
                
                if (escrowDetails.Status != "Active")
                {
                    throw new InvalidOperationException($"Escrow is not in active state. Current state: {escrowDetails.Status}");
                }
                
                if (string.IsNullOrEmpty(escrowDetails.OfferSequence))
                {
                    throw new InvalidOperationException("Escrow sequence is not available");
                }
                
                // Make sure we're past the CancelAfter time
                var currentRippleTime = ConvertToRippleTime(DateTime.UtcNow);
                uint cancelAfter = escrowDetails.FinishAfter + 86400; // 1 day after FinishAfter by default
                
                if (currentRippleTime < cancelAfter)
                {
                    throw new InvalidOperationException($"Escrow cannot be cancelled before its CancelAfter time ({cancelAfter}). Current Ripple time: {currentRippleTime}");
                }

                // Create EscrowCancel transaction according to XRPL specs
                var txJson = new
                {
                    TransactionType = "EscrowCancel",
                    Account = signerAddress,
                    Owner = escrowDetails.SourceAddress,
                    OfferSequence = uint.Parse(escrowDetails.OfferSequence)
                };

                var payloadJson = JsonSerializer.Serialize(txJson);
                var payload = new XummPostJsonPayload(payloadJson);
                payload.CustomMeta = new XummPayloadCustomMeta()
                {
                    Identifier = $"cancel_escrow_{escrowId}"
                };

                // Send to XUMM for signing
                var response = await _xummPayloadClient.CreateAsync(payload);
                
                if (response == null)
                {
                    throw new InvalidOperationException("Failed to create XUMM payload");
                }
                
                // Update escrow status
                escrowDetails.CancelPayloadId = response.Uuid;
                escrowDetails.Status = "CancelPending";
                escrowDetails.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                
                return new EscrowCancelResultDto
                {
                    Success = true,
                    EscrowId = escrowId,
                    XummPayloadId = response.Uuid,
                    QrCodeUrl = response.Refs.QrPng,
                    DeepLink = response.Next.Always
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling escrow {EscrowId}", escrowId);
                throw;
            }
        }

        /// <summary>
        /// Updates the escrow details after the transaction is confirmed on the ledger
        /// </summary>
        public async Task<bool> UpdateEscrowFromTransaction(string escrowId, string txHash)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(escrowId))
                    throw new ArgumentException("Escrow ID cannot be empty", nameof(escrowId));
                    
                if (string.IsNullOrWhiteSpace(txHash))
                    throw new ArgumentException("Transaction hash cannot be empty", nameof(txHash));
                
                var escrowDetails = await _dbContext.EscrowDetails.FindAsync(escrowId);
                
                if (escrowDetails == null)
                {
                    throw new ArgumentException($"Escrow with ID {escrowId} not found");
                }
                
                // Update with transaction details
                escrowDetails.TransactionHash = txHash;
                escrowDetails.Status = "Active";
                escrowDetails.UpdatedAt = DateTime.UtcNow;
                
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating escrow {EscrowId} with transaction details", escrowId);
                throw;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Converts a .NET DateTime to Ripple time format (seconds since Ripple epoch)
        /// </summary>
        private uint ConvertToRippleTime(DateTime dateTime)
        {
            // Ripple epoch starts on January 1, 2000 (946684800 Unix time)
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var rippleEpochInUnixSeconds = (uint)(rippleEpoch - unixEpoch).TotalSeconds;
            
            return (uint)(dateTime.ToUniversalTime() - unixEpoch).TotalSeconds - rippleEpochInUnixSeconds;
        }

        /// <summary>
        /// Converts XRP amount to drops (1 XRP = 1,000,000 drops)
        /// </summary>
        private string ConvertXrpToDrops(decimal xrp)
        {
            return ((ulong)(xrp * 1000000)).ToString();
        }

        /// <summary>
        /// Generates a cryptographic condition and fulfillment pair for PREIMAGE-SHA-256
        /// Following XRPL crypto-conditions specification
        /// </summary>
        private (string condition, string fulfillment) GenerateCryptoCondition()
        {
            // Create a random preimage (32 bytes)
            byte[] preimage = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(preimage);
            }
            
            // The fulfillment is the hex-encoded preimage
            string fulfillment = BitConverter.ToString(preimage).Replace("-", "").ToLowerInvariant();
            
            // The condition is the SHA-256 hash of the preimage with proper prefixes
            // A0258020 prefix indicates a PREIMAGE-SHA-256 condition type and length (32 bytes)
            byte[] hash = SHA256.HashData(preimage);
            string condition = "A0258020" + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            
            return (condition, fulfillment);
        }

        #endregion
    }
}