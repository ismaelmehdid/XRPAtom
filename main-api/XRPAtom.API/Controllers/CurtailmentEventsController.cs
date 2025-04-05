using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.Domain;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/curtailment-events")]
    [Authorize]
    public class CurtailmentEventsController : ControllerBase
    {
        private readonly ICurtailmentEventService _curtailmentService;
        private readonly IUserWalletService _userWalletService;
        private readonly IDeviceService _deviceService;
        private readonly IXRPLedgerService _xrplService;
        private readonly IBlockchainVerificationService _blockchainVerificationService;
        private readonly ILogger<CurtailmentEventsController> _logger;

        public CurtailmentEventsController(
            ICurtailmentEventService curtailmentService,
            IUserWalletService userWalletService,
            IDeviceService deviceService,
            IXRPLedgerService xrplService,
            IBlockchainVerificationService blockchainVerificationService,
            ILogger<CurtailmentEventsController> logger)
        {
            _curtailmentService = curtailmentService;
            _userWalletService = userWalletService;
            _deviceService = deviceService;
            _xrplService = xrplService;
            _blockchainVerificationService = blockchainVerificationService;
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
                bool blockchainVerified = false;
                if (!string.IsNullOrEmpty(eventDetails.BlockchainReference) && 
                    !string.IsNullOrEmpty(eventDetails.VerificationProof))
                {
                    blockchainVerified = await _blockchainVerificationService.VerifyEventOnBlockchain(
                        id, eventDetails.BlockchainReference, eventDetails.VerificationProof);
                }
                
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
                    blockchainVerified,
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

                // Check if user has a wallet
                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return BadRequest(new { error = "You must have a wallet to participate in curtailment events" });
                }

                // Check if user has registered devices
                var hasDevices = await _deviceService.HasEnrolledDevices(userId);
                if (!hasDevices)
                {
                    return BadRequest(new { error = "You must have at least one enrolled device to participate" });
                }

                // Check if the event exists and is in an appropriate state
                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                if (eventDetails.Status != EventStatus.Upcoming)
                {
                    return BadRequest(new { error = "You can only register for upcoming events" });
                }

                // Check if user is already registered
                if (await _curtailmentService.IsUserParticipant(eventId, userId))
                {
                    return BadRequest(new { error = "You are already registered for this event" });
                }

                // Register for the event
                var success = await _curtailmentService.RegisterUserForEventAsync(eventId, userId, wallet.Address);
                if (!success)
                {
                    return BadRequest(new { error = "Failed to register for event. It may be full or no longer accepting registrations." });
                }

                // Register the participation on the blockchain
                // This would typically be done in a background job
                try
                {
                    await _blockchainVerificationService.RecordEventParticipation(eventId, userId, wallet.Address);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to record participation on blockchain for user {UserId} in event {EventId}", userId, eventId);
                    // We continue anyway as this can be retried later
                }

                return Ok(new { success = true, message = "Successfully registered for the event" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering for curtailment event");
                return StatusCode(500, new { error = "An error occurred while registering for the event" });
            }
        }

        [HttpPost("claim-reward/{eventId}")]
        public async Task<IActionResult> ClaimReward(string eventId)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Check if user participated in the event
                var participation = await _curtailmentService.GetUserParticipation(eventId, userId);
                if (participation == null)
                {
                    return BadRequest(new { error = "You did not participate in this event" });
                }

                // Check if reward has already been claimed
                if (participation.RewardClaimed)
                {
                    return BadRequest(new { error = "Reward for this event has already been claimed" });
                }

                // Check if the event is completed and verified
                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails.Status != EventStatus.Completed)
                {
                    return BadRequest(new { error = "Event must be completed before claiming rewards" });
                }

                // Get user's wallet
                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return BadRequest(new { error = "No wallet found for this user" });
                }

                // Calculate reward based on participation
                var reward = participation.EnergySaved * eventDetails.RewardPerKwh;
                if (reward <= 0)
                {
                    return BadRequest(new { error = "No reward available for this participation" });
                }

                // Issue reward on the blockchain
                string transactionId;
                try
                {
                    transactionId = await _xrplService.IssueReward(wallet.Address, reward, eventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to issue reward on blockchain");
                    return StatusCode(500, new { error = "Failed to issue reward. Please try again later." });
                }

                // Update participation record
                await _curtailmentService.MarkRewardClaimed(eventId, userId, reward);

                // Update user's total rewards claimed
                await _userWalletService.UpdateTotalRewardsClaimedAsync(userId, reward);

                return Ok(new 
                { 
                    success = true, 
                    reward,
                    transactionId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error claiming reward for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while claiming the reward" });
            }
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

        [HttpPost("verify/{eventId}")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> VerifyEvent(string eventId, [FromBody] EventVerificationDto verification)
        {
            try
            {
                if (eventId != verification.EventId)
                {
                    return BadRequest(new { error = "Event ID mismatch" });
                }

                // Check if the event exists
                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                // Only active or completed events can be verified
                if (eventDetails.Status != EventStatus.Active && eventDetails.Status != EventStatus.Completed)
                {
                    return BadRequest(new { error = "Only active or completed events can be verified" });
                }

                // Verify the event on the blockchain
                string blockchainReference = await _blockchainVerificationService.CreateEventVerification(
                    eventId, verification.VerificationProof, verification.TotalEnergySaved);

                // Update the event with verification details
                var success = await _curtailmentService.SetBlockchainReferenceAsync(eventId, blockchainReference);
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to update event with blockchain reference" });
                }

                // Mark the event as completed if it's not already
                if (eventDetails.Status != EventStatus.Completed)
                {
                    await _curtailmentService.CompleteEventAsync(eventId);
                }

                return Ok(new 
                { 
                    success = true, 
                    blockchainReference,
                    message = "Event verified successfully on the blockchain"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while verifying the event" });
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateCurtailmentEventDto createEventDto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Validate the request
                if (createEventDto.StartTime >= createEventDto.EndTime)
                {
                    return BadRequest(new { error = "End time must be after start time" });
                }

                if (createEventDto.StartTime < DateTime.UtcNow)
                {
                    return BadRequest(new { error = "Start time must be in the future" });
                }

                // Create the event
                var createdEvent = await _curtailmentService.CreateEventAsync(createEventDto);

                return Ok(createdEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating curtailment event");
                return StatusCode(500, new { error = "An error occurred while creating the event" });
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
    }
}