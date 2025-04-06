using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.Domain;
using XRPAtom.Infrastructure.Data;
using XUMM.NET.SDK.Clients.Interfaces;
using XUMM.NET.SDK.Models.Payload;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/curtailment-events")]
    [Authorize]
    public class CurtailmentEventsController : ControllerBase
    {
        private readonly ICurtailmentEventService _curtailmentService;
        private readonly IDeviceService _deviceService;
        private readonly IEscrowService _escrowService;
        private readonly ILogger<CurtailmentEventsController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public CurtailmentEventsController(
            ICurtailmentEventService curtailmentService,
            IDeviceService deviceService,
            IEscrowService escrowService,
            ApplicationDbContext dbContext,
            ILogger<CurtailmentEventsController> logger)
        {
            _curtailmentService = curtailmentService;
            _deviceService = deviceService;
            _escrowService = escrowService;
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                var events = await _curtailmentService.GetUserEventsAsync(userId, page, limit);
                var total = await _curtailmentService.GetUserEventCountAsync(userId);

                return Ok(new
                {
                    events,
                    pagination = new
                    {
                        total,
                        page,
                        limit,
                        pages = (int)Math.Ceiling((double)total / limit)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving curtailment events");
                return StatusCode(500, new { error = "An error occurred while retrieving curtailment events" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventDetails(string id)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                var eventDetails = await _curtailmentService.GetEventByIdAsync(id);
                
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }
                
                if (userId == null)
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Check if user is authorized to view this event
                // For TSO/operators, they can see all events
                // For residential users, they can only see events they're participating in
                var userRole = User.FindFirst("role")?.Value;
                if (userRole != "TSO" && userRole != "Admin")
                {
                    bool isParticipant = await _curtailmentService.IsUserParticipant(id, userId);
                    if (!isParticipant)
                    {
                        return Forbid();
                    }
                }

                // Get blockchain verification details if available
                
                // Get user's participation if they're participating
                EventParticipationDto? participation = null;
                if (await _curtailmentService.IsUserParticipant(id, userId))
                {
                    participation = await _curtailmentService.GetUserParticipation(id, userId);
                }
                
                // Enrich response with blockchain verification and participation
                var response = new
                {
                    eventDetails,
                    userParticipation = participation,
                    canClaimRewards = participation != null && 
                                      !participation.RewardClaimed && 
                                      eventDetails.Status == EventStatus.Completed
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving curtailment event details");
                return StatusCode(500, new { error = "An error occurred while retrieving event details" });
            }
        }

        [HttpPost("register/{eventId}")]
        public async Task<IActionResult> RegisterForEvent(string eventId)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Basic validation (existing code)
                var user = await _dbContext.Users
                    .Include(u => u.Wallet)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user?.Wallet == null)
                {
                    return BadRequest(new { error = "User wallet not found" });
                }

                var hasDevices = await _deviceService.HasEnrolledDevices(userId);
                if (!hasDevices)
                {
                    return BadRequest(new { error = "You must have at least one enrolled device to participate" });
                }

                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                if (eventDetails.Status != EventStatus.Upcoming)
                {
                    return BadRequest(new { error = "You can only register for upcoming events" });
                }

                if (await _curtailmentService.IsUserParticipant(eventId, userId))
                {
                    return BadRequest(new { error = "You are already registered for this event" });
                }

                // Register the user for the event
                var success = await _curtailmentService.RegisterUserForEventAsync(eventId, userId, user.Wallet.Address);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to register for event" });
                }
                
                // Calculate potential reward based on user's device capacity
                var deviceCapacity = await _deviceService.GetTotalCurtailmentCapacityAsync(userId);
                var potentialReward = Convert.ToDecimal(deviceCapacity) * eventDetails.RewardPerKwh;
                
                // Get event creator's wallet for escrow creation
                var creator = await _dbContext.Users
                    .Include(u => u.Wallet)
                    .FirstOrDefaultAsync(u => u.Id == eventDetails.CreatedBy);
                    
                if (creator?.Wallet == null)
                {
                    return BadRequest(new { error = "Event creator wallet not found" });
                }
                
                // Set the release date 3 days after event ends
                DateTime releaseDate = eventDetails.EndTime.AddDays(3);
                
                // Get the main escrow for this event
                var mainEscrow = await _dbContext.EscrowDetails
                    .FirstOrDefaultAsync(e => e.EventId == eventId && e.EscrowType == "MainEvent");
                
                if (mainEscrow == null || mainEscrow.Status != "Active")
                {
                    // No active main escrow - we'll still register the user but without an escrow
                    return Ok(new
                    {
                        success = true,
                        message = "Registered for event successfully, but no active escrow found for automatic rewards",
                        potential_reward = potentialReward
                    });
                }
                
                // Create individual escrow for this participant
                var escrowResult = await _escrowService.CreateParticipantEscrow(
                    eventId,
                    userId,
                    creator.Wallet.Address,
                    user.Wallet.Address,
                    potentialReward,
                    releaseDate);
                
                return Ok(new
                {
                    success = true,
                    message = "Successfully registered for the event",
                    potential_reward = potentialReward,
                    escrow_details = new
                    {
                        escrow_id = escrowResult.EscrowId,
                        xumm_payload = escrowResult.XummPayloadId,
                        qr_code = escrowResult.QrCodeUrl,
                        deep_link = escrowResult.DeepLink
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering for curtailment event");
                return StatusCode(500, new { error = "An error occurred while registering for the event" });
            }
        }
        
        [HttpPost("verify-performance/{eventId}/{participantId}")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> VerifyParticipantPerformance(
            string eventId, 
            string participantId,
            [FromBody] VerifyPerformanceDto verificationDto)
        {
            try
            {
                var operatorId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(operatorId))
                {
                    return BadRequest(new { error = "Invalid operator identifier" });
                }
                // Get the event and verify it's in a valid state
                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                if (eventDetails.Status != EventStatus.Completed && eventDetails.Status != EventStatus.Active)
                {
                    return BadRequest(new { error = "Event must be active or completed to verify performance" });
                }

                // Get participation details
                var participation = await _curtailmentService.GetUserParticipation(eventId, participantId);
                if (participation == null)
                {
                    return NotFound(new { error = "Participation record not found" });
                }

                // Record the energy saved
                await _curtailmentService.RecordEnergySavedAsync(eventId, participantId, verificationDto.EnergySaved);

                // Calculate the actual reward based on performance
                decimal actualReward = verificationDto.EnergySaved * eventDetails.RewardPerKwh;

                // Get the participant's escrow from the database
                var participantEscrow = await _dbContext.EscrowDetails
                    .FirstOrDefaultAsync(e => 
                        e.EventId == eventId && 
                        e.ParticipantId == participantId && 
                        e.EscrowType == "Participant" &&
                        e.Status == "Active");

                if (participantEscrow == null)
                {
                    // No active escrow found - update the database but can't release funds automatically
                    await _curtailmentService.VerifyParticipationAsync(eventId, participantId);
                    
                    if (verificationDto.SuccessfullyMet)
                    {
                        await _curtailmentService.MarkRewardClaimed(eventId, participantId, actualReward);
                    }
                    
                    return Ok(new
                    {
                        success = true,
                        energy_saved = verificationDto.EnergySaved,
                        reward_amount = verificationDto.SuccessfullyMet ? actualReward : 0,
                        message = "Performance verified, but no active escrow found for automatic reward"
                    });
                }

                // Get operator wallet for signing
                var operator_ = await _dbContext.Users
                    .Include(u => u.Wallet)
                    .FirstOrDefaultAsync(u => u.Id == operatorId);
                    
                if (operator_?.Wallet == null)
                {
                    return BadRequest(new { error = "Operator wallet not found" });
                }

                // Release or cancel the escrow based on performance
                if (verificationDto.SuccessfullyMet)
                {
                    var finishResult = await _escrowService.FinishEscrow(
                        participantEscrow.Id,
                        operator_.Wallet.Address);
                        
                    // Mark the reward as claimed
                    await _curtailmentService.VerifyParticipationAsync(eventId, participantId);
                    await _curtailmentService.MarkRewardClaimed(eventId, participantId, actualReward);
                    
                    return Ok(new
                    {
                        success = true,
                        energy_saved = verificationDto.EnergySaved,
                        reward_amount = actualReward,
                        message = "Performance verification successful, escrow release initiated",
                        escrow_details = new
                        {
                            escrow_id = participantEscrow.Id,
                            transaction_type = "EscrowFinish",
                            xumm_payload = finishResult.XummPayloadId,
                            qr_code = finishResult.QrCodeUrl,
                            deep_link = finishResult.DeepLink
                        }
                    });
                }
                else
                {
                    // For cancellation, we need to wait until after the FinishAfter time
                    var currentTime = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    
                    if (currentTime < participantEscrow.FinishAfter)
                    {
                        return BadRequest(new 
                        { 
                            error = "Cannot cancel escrow before its FinishAfter time",
                            current_time = currentTime,
                            finish_after = participantEscrow.FinishAfter
                        });
                    }
                    
                    var cancelResult = await _escrowService.CancelEscrow(
                        participantEscrow.Id,
                        operator_.Wallet.Address);
                        
                    // Mark the verification as processed but no reward
                    await _curtailmentService.VerifyParticipationAsync(eventId, participantId);
                    
                    return Ok(new
                    {
                        success = true,
                        energy_saved = verificationDto.EnergySaved,
                        reward_amount = 0,
                        message = "Performance verification failed, escrow cancellation initiated",
                        escrow_details = new
                        {
                            escrow_id = participantEscrow.Id,
                            transaction_type = "EscrowCancel",
                            xumm_payload = cancelResult.XummPayloadId,
                            qr_code = cancelResult.QrCodeUrl,
                            deep_link = cancelResult.DeepLink
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying participant performance for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred during performance verification" });
            }
        }
        
        [HttpPost("escrow-transaction-update")]
        [AllowAnonymous] // This endpoint is called by a webhook
        public async Task<IActionResult> UpdateEscrowTransaction([FromBody] EscrowTransactionUpdateDto updateDto)
        {
            try
            {
                // Validate the webhook signature (important for production!)
                // This would typically use a shared secret or API key
                // Find the escrow by the XUMM payload ID
                var escrow = await _dbContext.EscrowDetails
                    .FirstOrDefaultAsync(e => e.XummPayloadId == updateDto.PayloadId);
        
                if (escrow == null)
                {
                    return NotFound(new { error = "Escrow not found for payload" });
                }
    
                // Update the escrow with transaction details
                await _escrowService.UpdateEscrowFromTransaction(
                    escrow.Id,
                    updateDto.TransactionHash
                );
        
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating escrow transaction");
                return StatusCode(500, new { error = "An error occurred while updating escrow transaction" });
            }
        }
        
        public class CreateCurtailmentEventWithEscrowDto : CreateCurtailmentEventDto
        {
            public int EstimatedParticipants { get; set; }
            public decimal AverageEnergySavingsPerUser { get; set; } // in kWh
        }
        public class VerifyPerformanceDto
        {
            public decimal EnergySaved { get; set; }
            public bool SuccessfullyMet { get; set; }
        }
        public class EscrowTransactionUpdateDto
        {
            public string PayloadId { get; set; }
            public string TransactionHash { get; set; }
            public uint OfferSequence { get; set; }
        }
        
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingEvents([FromQuery] int hours = 24, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var events = await _curtailmentService.GetUpcomingEventsAsync(hours, page, limit);
                
                return Ok(new { events });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming events");
                return StatusCode(500, new { error = "An error occurred while retrieving upcoming events" });
            }
        }

        [HttpPost("create-with-funding")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> CreateEventWithFunding([FromBody] CreateCurtailmentEventWithEscrowDto createEventDto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Validate request
                if (createEventDto.StartTime >= createEventDto.EndTime)
                {
                    return BadRequest(new { error = "End time must be after start time" });
                }

                if (createEventDto.StartTime < DateTime.UtcNow)
                {
                    return BadRequest(new { error = "Start time must be in the future" });
                }
                
                if (createEventDto.RewardPerKwh <= 0)
                {
                    return BadRequest(new { error = "Reward per kWh must be greater than zero" });
                }
                
                if (createEventDto.EstimatedParticipants <= 0)
                {
                    return BadRequest(new { error = "Estimated participants must be greater than zero" });
                }

                // Get the creator's wallet address from the database
                var creator = await _dbContext.Users
                    .Include(u => u.Wallet)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (creator?.Wallet == null)
                {
                    return BadRequest(new { error = "Creator wallet not found" });
                }

                // First create the event in the database
                var baseDto = new CreateCurtailmentEventDto
                {
                    Title = createEventDto.Title,
                    Description = createEventDto.Description,
                    StartTime = createEventDto.StartTime,
                    EndTime = createEventDto.EndTime,
                    RewardPerKwh = createEventDto.RewardPerKwh,
                    CreatedBy = userId
                };
                
                var createdEvent = await _curtailmentService.CreateEventAsync(baseDto);
                
                // Calculate the total reward pool
                decimal totalRewardPool = createEventDto.EstimatedParticipants * 
                                          createEventDto.AverageEnergySavingsPerUser * 
                                          createEventDto.RewardPerKwh;
                
                // Set the release date 3 days after event ends
                DateTime releaseDate = createEventDto.EndTime.AddDays(3);
                
                // Create the main escrow for the event
                var escrowResult = await _escrowService.CreateMainEventEscrow(
                    createdEvent.Id,
                    creator.Wallet.Address,
                    totalRewardPool,
                    releaseDate);
                
                // Store escrow reference in the event
                await _curtailmentService.SetBlockchainReferenceAsync(createdEvent.Id, escrowResult.EscrowId);
                
                return Ok(new
                {
                    event_details = createdEvent,
                    escrow_details = new
                    {
                        escrow_id = escrowResult.EscrowId,
                        total_pool = totalRewardPool,
                        release_date = releaseDate,
                        xumm_payload = escrowResult.XummPayloadId,
                        qr_code = escrowResult.QrCodeUrl,
                        deep_link = escrowResult.DeepLink
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating curtailment event with funding");
                return StatusCode(500, new { error = "An error occurred while creating the event with funding" });
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> UpdateEvent(string id, [FromBody] UpdateCurtailmentEventDto updateEventDto)
        {
            try
            {
                // Check if the event exists
                var eventDetails = await _curtailmentService.GetEventByIdAsync(id);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                // Only upcoming events can be updated
                if (eventDetails.Status != EventStatus.Upcoming)
                {
                    return BadRequest(new { error = "Only upcoming events can be updated" });
                }

                // Update the event
                var updatedEvent = await _curtailmentService.UpdateEventAsync(id, updateEventDto);

                return Ok(updatedEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating curtailment event {EventId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the event" });
            }
        }

        [HttpPost("{id}/start")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> StartEvent(string id)
        {
            try
            {
                // Check if the event exists
                var eventDetails = await _curtailmentService.GetEventByIdAsync(id);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                // Only upcoming events can be started
                if (eventDetails.Status != EventStatus.Upcoming)
                {
                    return BadRequest(new { error = "Only upcoming events can be started" });
                }

                // Start the event
                var success = await _curtailmentService.StartEventAsync(id);
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to start the event" });
                }

                return Ok(new { success = true, message = "Event started successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting curtailment event {EventId}", id);
                return StatusCode(500, new { error = "An error occurred while starting the event" });
            }
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> CompleteEvent(string id)
        {
            try
            {
                // Check if the event exists
                var eventDetails = await _curtailmentService.GetEventByIdAsync(id);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                // Only active events can be completed
                if (eventDetails.Status != EventStatus.Active)
                {
                    return BadRequest(new { error = "Only active events can be completed" });
                }

                // Complete the event
                var success = await _curtailmentService.CompleteEventAsync(id);
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to complete the event" });
                }

                return Ok(new { success = true, message = "Event completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing curtailment event {EventId}", id);
                return StatusCode(500, new { error = "An error occurred while completing the event" });
            }
        }

        [HttpGet("participants/{eventId}")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> GetEventParticipants(string eventId, [FromQuery] int page = 1, [FromQuery] int limit = 25)
        {
            try
            {
                // Check if the event exists
                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                // Get the participants
                var participants = await _curtailmentService.GetEventParticipantsAsync(eventId, page, limit);

                return Ok(new { participants });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving participants for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while retrieving event participants" });
            }
        }
        
        // Helper methods for XRPL escrow functionality
        
        private uint ToRippleTime(DateTime dateTime)
        {
            // Ripple epoch is January 1, 2000 (946684800 Unix time)
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var rippleEpochInUnixSeconds = (uint)(rippleEpoch - unixEpoch).TotalSeconds;
            
            return (uint)(dateTime.ToUniversalTime() - unixEpoch).TotalSeconds - rippleEpochInUnixSeconds;
        }

        private string GenerateEscrowCondition(string eventTitle)
        {
            // In a real implementation, you would create a secure cryptographic condition
            // This is a placeholder that would be replaced with actual crypto condition generation
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"Event:{eventTitle}-{Guid.NewGuid()}"));
        }

        private string GenerateParticipantCondition(string eventId, string userId)
        {
            // In a real implementation, this would generate a unique condition for the participant
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"Participant:{eventId}-{userId}-{Guid.NewGuid()}"));
        }

        private string GenerateFulfillment(string condition)
        {
            // In a real implementation, this would generate the fulfillment that satisfies the condition
            // This is complex cryptography that depends on the type of condition used
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"Fulfillment-for-{condition}"));
        }
    }
}