using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.Domain;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;
using XUMM.NET.SDK.Clients.Interfaces;

namespace XRPAtom.BackgroundServices
{
    /// <summary>
    /// Background service to monitor and manage escrow transactions
    /// Handles checking escrow status, automatic processing, and lifecycle management
    /// </summary>
    public class EscrowMonitorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EscrowMonitorService> _logger;

        public EscrowMonitorService(
            IServiceProvider serviceProvider,
            ILogger<EscrowMonitorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Escrow Monitor Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingEscrows();
                    await ProcessActiveEscrows();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Escrow Monitor Service main loop");
                }

                // Check every 15 minutes
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }

            _logger.LogInformation("Escrow Monitor Service is stopping");
        }

        /// <summary>
        /// Process escrows that are in a pending state (waiting for initial creation confirmation)
        /// </summary>
        private async Task ProcessPendingEscrows()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var xummClient = scope.ServiceProvider.GetRequiredService<IXummPayloadClient>();
            var escrowService = scope.ServiceProvider.GetRequiredService<IEscrowService>();

            // Find pending escrows from the last 24 hours
            var cutoffTime = DateTime.UtcNow.AddHours(-24);
            var pendingEscrows = await dbContext.EscrowDetails
                .Where(e => e.Status == "Pending" && e.CreatedAt > cutoffTime)
                .ToListAsync();

            foreach (var escrow in pendingEscrows)
            {
                try
                {
                    // Check XUMM payload status
                    var payloadDetails = await xummClient.GetAsync(escrow.XummPayloadId);

                    if (payloadDetails == null)
                    {
                        _logger.LogWarning("Payload {PayloadId} not found for escrow {EscrowId}", 
                            escrow.XummPayloadId, escrow.Id);
                        continue;
                    }

                    // If payload is resolved and signed
                    if (payloadDetails.Meta.Resolved && 
                        payloadDetails.Meta.Signed && 
                        !string.IsNullOrEmpty(payloadDetails.Response?.Txid))
                    {
                        // Update escrow with transaction details
                        // Retrieve full transaction details from XRPL
                        var txid = payloadDetails.Response.Txid;
                        var request = JsonSerializer.Deserialize<JsonElement>(payloadDetails.Payload.RequestJson.RootElement);
                        
                        // Update escrow with transaction details
                        await escrowService.UpdateEscrowFromTransaction(
                            escrow.Id, 
                            payloadDetails.Response.Txid
                        );

                        _logger.LogInformation("Escrow {EscrowId} confirmed active with transaction {txid}", 
                            escrow.Id, payloadDetails.Response.Txid);
                    }
                    else if (payloadDetails.Meta.Expired || 
                             (payloadDetails.Meta.Resolved && !payloadDetails.Meta.Signed))
                    {
                        // Mark as failed if expired or rejected
                        escrow.Status = "Failed";
                        escrow.UpdatedAt = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync();

                        _logger.LogWarning("Escrow {EscrowId} failed - payload expired or rejected", escrow.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing pending escrow {EscrowId}", escrow.Id);
                }
            }
        }

        /// <summary>
        /// Process active escrows that may need finishing or cancellation
        /// </summary>
        private async Task ProcessActiveEscrows()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var escrowService = scope.ServiceProvider.GetRequiredService<IEscrowService>();

            // Current Ripple time (seconds since Ripple epoch)
            var currentRippleTime = ConvertToRippleTime(DateTime.UtcNow);

            // Find active participant escrows that have reached their finish time
            var activeEscrows = await dbContext.EscrowDetails
                .Include(e => e.Event) // Include event to check participation status
                .Where(e => e.Status == "Active" && 
                            e.EscrowType == "Participant" && 
                            e.FinishAfter < currentRippleTime)
                .ToListAsync();

            foreach (var escrow in activeEscrows)
            {
                try
                {
                    // Find the event creator to sign escrow finish/cancel
                    var creator = await dbContext.Users
                        .Include(u => u.Wallet)
                        .FirstOrDefaultAsync(u => u.Id == escrow.Event.CreatedBy);

                    if (creator?.Wallet == null)
                    {
                        _logger.LogWarning("No creator wallet found for event {EventId}", escrow.EventId);
                        continue;
                    }

                    // Get participation details
                    var participation = await dbContext.EventParticipations
                        .FirstOrDefaultAsync(p => 
                            p.EventId == escrow.EventId && 
                            p.UserId == escrow.ParticipantId);

                    if (participation == null)
                    {
                        _logger.LogWarning("No participation found for event {EventId}, participant {ParticipantId}", 
                            escrow.EventId, escrow.ParticipantId);
                        continue;
                    }

                    // Determine whether to finish or cancel based on participation
                    if (participation.Status == ParticipationStatus.Verified && 
                        participation.EnergySaved > 0)
                    {
                        // Successfully met curtailment target - finish the escrow
                        var finishResult = await escrowService.FinishEscrow(
                            escrow.Id, 
                            creator.Wallet.Address
                        );

                        _logger.LogInformation(
                            "Automatically finishing escrow {EscrowId} for verified participant with {EnergySaved} kWh", 
                            escrow.Id, participation.EnergySaved
                        );
                    }
                    else
                    {
                        // Did not meet target - cancel the escrow
                        var cancelResult = await escrowService.CancelEscrow(
                            escrow.Id, 
                            creator.Wallet.Address
                        );

                        _logger.LogInformation(
                            "Automatically cancelling escrow {EscrowId} for unverified participant", 
                            escrow.Id
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing active escrow {EscrowId}", escrow.Id);
                }
            }
        }

        /// <summary>
        /// Converts a .NET DateTime to Ripple time format (seconds since Ripple epoch)
        /// </summary>
        private uint ConvertToRippleTime(DateTime dateTime)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var rippleEpochInUnixSeconds = (uint)(rippleEpoch - unixEpoch).TotalSeconds;
            
            return (uint)(dateTime.ToUniversalTime() - unixEpoch).TotalSeconds - rippleEpochInUnixSeconds;
        }
    }
}