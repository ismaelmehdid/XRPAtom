using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEscrowService _escrowService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            ApplicationDbContext dbContext,
            IEscrowService escrowService,
            ILogger<WebhookController> logger)
        {
            _dbContext = dbContext;
            _escrowService = escrowService;
            _logger = logger;
        }

        [HttpPost("xumm")]
        public async Task<IActionResult> XummWebhook([FromBody] XummWebhookDto webhook)
        {
            try
            {
                _logger.LogInformation("Received XUMM webhook for payload {PayloadId}", webhook.PayloadUuid);
                
                // Find any escrows associated with this payload
                var escrow = await _dbContext.EscrowDetails
                    .FirstOrDefaultAsync(e => 
                        e.XummPayloadId == webhook.PayloadUuid || 
                        e.FinishPayloadId == webhook.PayloadUuid || 
                        e.CancelPayloadId == webhook.PayloadUuid);
                        
                if (escrow == null)
                {
                    _logger.LogWarning("No escrow found for payload {PayloadId}", webhook.PayloadUuid);
                    return Ok(); // Return 200 to acknowledge receipt
                }
                
                // Check if the payload was signed and has a transaction
                if (webhook.PayloadStatus == "signed" && !string.IsNullOrEmpty(webhook.TransactionId))
                {
                    if (escrow.XummPayloadId == webhook.PayloadUuid)
                    {
                        // This is the initial EscrowCreate
                        // We need to extract the sequence from the transaction
                        uint offerSequence = await GetEscrowSequence(webhook.TransactionId);
                        
                        await _escrowService.UpdateEscrowFromTransaction(
                            escrow.Id,
                            webhook.TransactionId
                        );
                            
                        _logger.LogInformation("Updated escrow {EscrowId} with transaction {TxId} and sequence {Sequence}", 
                            escrow.Id, webhook.TransactionId, offerSequence);
                    }
                    else if (escrow.FinishPayloadId == webhook.PayloadUuid)
                    {
                        // This is an EscrowFinish
                        escrow.Status = "Finished";
                        escrow.UpdatedAt = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();
                        
                        _logger.LogInformation("Marked escrow {EscrowId} as finished", escrow.Id);
                    }
                    else if (escrow.CancelPayloadId == webhook.PayloadUuid)
                    {
                        // This is an EscrowCancel
                        escrow.Status = "Cancelled";
                        escrow.UpdatedAt = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();
                        
                        _logger.LogInformation("Marked escrow {EscrowId} as cancelled", escrow.Id);
                    }
                }
                else if (webhook.PayloadStatus == "rejected")
                {
                    _logger.LogWarning("Payload {PayloadId} was rejected for escrow {EscrowId}", 
                        webhook.PayloadUuid, escrow.Id);
                        
                    // Mark as failed if it's the initial creation
                    if (escrow.XummPayloadId == webhook.PayloadUuid && escrow.Status == "Pending")
                    {
                        escrow.Status = "Failed";
                        escrow.UpdatedAt = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();
                    }
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing XUMM webhook for payload {PayloadId}", webhook.PayloadUuid);
                return StatusCode(500);
            }
        }
        
        // Helper method to get the escrow sequence from a transaction
        private async Task<uint> GetEscrowSequence(string transactionId)
        {
            // In a real implementation, you would query the XRPL API to get the sequence
            // For simplicity, we'll return a placeholder value
            return 12345;
        }
    }

    public class XummWebhookDto
    {
        public string PayloadUuid { get; set; }
        public string PayloadStatus { get; set; } // "signed", "rejected", "expired"
        public string TransactionId { get; set; }
        public string UserToken { get; set; }
    }
}