using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/rewards")]
    [Authorize]
    public class RewardController : ControllerBase
    {
        private readonly ICurtailmentRewardManager _rewardManager;
        private readonly ICurtailmentEventService _curtailmentService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<RewardController> _logger;

        public RewardController(
            ICurtailmentRewardManager rewardManager,
            ICurtailmentEventService curtailmentService,
            ApplicationDbContext dbContext,
            ILogger<RewardController> logger)
        {
            _rewardManager = rewardManager;
            _curtailmentService = curtailmentService;
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("events/{eventId}/fund")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> FundEvent(string eventId, [FromBody] FundEventRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Validate input
                if (request.Amount <= 0)
                {
                    return BadRequest(new { error = "Amount must be greater than zero" });
                }

                // Get the event
                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                // Get the grid operator's wallet
                var gridOperator = await _dbContext.Users
                    .Include(u => u.Wallet)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (gridOperator?.Wallet == null)
                {
                    return BadRequest(new { error = "Grid operator wallet not found" });
                }

                // Create funding pool
                var result = await _rewardManager.CreateEventFundingPool(
                    eventId, 
                    gridOperator.Wallet.Address, 
                    request.Amount);

                return Ok(new
                {
                    success = true,
                    poolId = result.PoolId,
                    transactionHash = result.TransactionHash,
                    amount = result.TotalAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error funding event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while funding the event" });
            }
        }

        [HttpPost("events/{eventId}/allocate")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> AllocateRewards(string eventId)
        {
            try
            {
                // Get the event
                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                // Get all participants for this event
                var participants = await _dbContext.EventParticipations
                    .Where(p => p.EventId == eventId)
                    .Select(p => p.UserId)
                    .ToListAsync();

                if (!participants.Any())
                {
                    return BadRequest(new { error = "No participants found for this event" });
                }

                // Allocate rewards
                var success = await _rewardManager.AllocateParticipantRewards(eventId, participants);

                return Ok(new
                {
                    success,
                    participantCount = participants.Count,
                    message = "Reward allocations created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating rewards for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while allocating rewards" });
            }
        }

        [HttpPost("events/{eventId}/finalize")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> FinalizeRewards(string eventId)
        {
            try
            {
                // Get the event
                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                if (eventDetails.Status != Core.Domain.EventStatus.Completed)
                {
                    return BadRequest(new { error = "Event must be completed before rewards can be finalized" });
                }

                // Finalize rewards
                var success = await _rewardManager.FinalizeRewards(eventId);

                return Ok(new
                {
                    success,
                    message = "Rewards finalized successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing rewards for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while finalizing rewards" });
            }
        }

        [HttpPost("payments/{payloadId}/process")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> ProcessPayment(string payloadId, [FromBody] ProcessPaymentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TransactionHash))
                {
                    return BadRequest(new { error = "Transaction hash is required" });
                }

                // Process the payment
                var success = await _rewardManager.ProcessRewardPayment(payloadId, request.TransactionHash);

                return Ok(new
                {
                    success,
                    message = "Payment processed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for payload {PayloadId}", payloadId);
                return StatusCode(500, new { error = "An error occurred while processing the payment" });
            }
        }

        [HttpGet("events/{eventId}/allocations")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> GetEventAllocations(string eventId)
        {
            try
            {
                // Get the event
                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                // Get all allocations for this event
                var allocations = await _dbContext.RewardAllocations
                    .Include(a => a.Participant)
                    .Where(a => a.EventId == eventId)
                    .Select(a => new
                    {
                        id = a.Id,
                        participantId = a.ParticipantId,
                        participantName = a.Participant.Name,
                        potentialAmount = a.PotentialAmount,
                        actualAmount = a.ActualAmount,
                        status = a.Status,
                        createdAt = a.CreatedAt,
                        verifiedAt = a.VerifiedAt
                    })
                    .ToListAsync();

                return Ok(allocations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving allocations for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while retrieving allocations" });
            }
        }

        [HttpGet("events/{eventId}/payments")]
        [Authorize(Roles = "TSO,Admin,GridOperator")]
        public async Task<IActionResult> GetEventPayments(string eventId)
        {
            try
            {
                // Get the event
                var eventDetails = await _curtailmentService.GetEventByIdAsync(eventId);
                if (eventDetails == null)
                {
                    return NotFound(new { error = "Event not found" });
                }

                // Get all payments for this event
                var payments = await _dbContext.RewardPayments
                    .Include(p => p.Participant)
                    .Where(p => p.EventId == eventId)
                    .Select(p => new
                    {
                        id = p.Id,
                        participantId = p.ParticipantId,
                        participantName = p.Participant.Name,
                        amount = p.Amount,
                        payloadId = p.PayloadId,
                        transactionHash = p.TransactionHash,
                        status = p.Status,
                        createdAt = p.CreatedAt,
                        completedAt = p.CompletedAt
                    })
                    .ToListAsync();

                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for event {EventId}", eventId);
                return StatusCode(500, new { error = "An error occurred while retrieving payments" });
            }
        }

        [HttpGet("user/allocations")]
        public async Task<IActionResult> GetUserAllocations()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Get all allocations for this user
                var allocations = await _dbContext.RewardAllocations
                    .Include(a => a.Event)
                    .Where(a => a.ParticipantId == userId)
                    .Select(a => new
                    {
                        id = a.Id,
                        eventId = a.EventId,
                        eventTitle = a.Event.Title,
                        potentialAmount = a.PotentialAmount,
                        actualAmount = a.ActualAmount,
                        status = a.Status,
                        createdAt = a.CreatedAt,
                        verifiedAt = a.VerifiedAt
                    })
                    .ToListAsync();

                return Ok(allocations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving allocations for user");
                return StatusCode(500, new { error = "An error occurred while retrieving allocations" });
            }
        }

        [HttpGet("user/payments")]
        public async Task<IActionResult> GetUserPayments()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Get all payments for this user
                var payments = await _dbContext.RewardPayments
                    .Include(p => p.Event)
                    .Where(p => p.ParticipantId == userId)
                    .Select(p => new
                    {
                        id = p.Id,
                        eventId = p.EventId,
                        eventTitle = p.Event.Title,
                        amount = p.Amount,
                        transactionHash = p.TransactionHash,
                        status = p.Status,
                        createdAt = p.CreatedAt,
                        completedAt = p.CompletedAt
                    })
                    .ToListAsync();

                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for user");
                return StatusCode(500, new { error = "An error occurred while retrieving payments" });
            }
        }
    }

    public class FundEventRequest
    {
        public decimal Amount { get; set; }
    }

    public class ProcessPaymentRequest
    {
        public string TransactionHash { get; set; }
    }
}