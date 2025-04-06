using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.Blockchain.Services;

public class BlockchainEventListener : IBlockchainEventListener
{
    private readonly IXRPLedgerService _xrplService;
    private readonly ILogger<BlockchainEventListener> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IEscrowService _escrowService;

    public BlockchainEventListener(
        IXRPLedgerService xrplService,
        ApplicationDbContext dbContext,
        IEscrowService escrowService,
        ILogger<BlockchainEventListener> logger)
    {
        _xrplService = xrplService;
        _dbContext = dbContext;
        _escrowService = escrowService;
        _logger = logger;
    }

    public async Task<bool> ProcessEscrowTransaction(string payloadId, string transactionHash, uint offerSequence)
    {
        try
        {
            // Find the escrow by the payload ID
            var escrow = await _dbContext.EscrowDetails
                .FirstOrDefaultAsync(e => e.XummPayloadId == payloadId);
                
            if (escrow == null)
            {
                _logger.LogWarning("Escrow not found for payload {PayloadId}", payloadId);
                return false;
            }
            
            // Update the escrow with transaction details
            return await _escrowService.UpdateEscrowFromTransaction(
                escrow.Id,
                transactionHash
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing escrow transaction for payload {PayloadId}", payloadId);
            return false;
        }
    }
    
    public async Task<bool> MonitorLedgerForEscrowEvents()
    {
        try 
        {
            // Get current ledger
            var ledgerResponse = await _xrplService.GetLedgerCurrent();
            var ledgerData = JsonDocument.Parse(ledgerResponse);
            var currentLedger = ledgerData.RootElement
                .GetProperty("result")
                .GetProperty("ledger_current_index")
                .GetUInt32();
                
            _logger.LogInformation("Current ledger index: {LedgerIndex}", currentLedger);
            
            // In a production system, you would now use subscribe to ledger events
            // or poll regularly for transactions related to your escrows
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring ledger for escrow events");
            return false;
        }
    }
    
    public async Task<bool> NotifyEventCompletion(string eventId, string transactionHash)
    {
        try
        {
            // Find all escrows related to this event
            var escrows = await _dbContext.EscrowDetails
                .Where(e => e.EventId == eventId)
                .ToListAsync();
                
            _logger.LogInformation("Found {Count} escrows for event {EventId}", 
                escrows.Count, eventId);
                
            // In a production system, you would now trigger notifications to users
            // or start automated processes to finalize the escrows
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying event completion for event {EventId}", eventId);
            return false;
        }
    }
}