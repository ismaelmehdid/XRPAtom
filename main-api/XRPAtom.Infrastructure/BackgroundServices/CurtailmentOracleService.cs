using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;
using XUMM.NET.SDK.Clients.Interfaces;
using XUMM.NET.SDK.Models.Payload;

namespace XRPAtom.Infrastructure.BackgroundServices
{
    public class CurtailmentOracleService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CurtailmentOracleService> _logger;

        public CurtailmentOracleService(
            IServiceProvider serviceProvider,
            ILogger<CurtailmentOracleService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Curtailment Oracle Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessCompletedEvents();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Curtailment Oracle Service");
                }

                // Check every hour for events that need processing
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("Curtailment Oracle Service is stopping");
        }

        private async Task ProcessCompletedEvents()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var curtailmentService = scope.ServiceProvider.GetRequiredService<ICurtailmentEventService>();
            var userWalletService = scope.ServiceProvider.GetRequiredService<IUserWalletService>();
            var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceService>();
            var xummClient = scope.ServiceProvider.GetRequiredService<IXummPayloadClient>();

            // Find events that have ended but haven't been fully processed
            var completedEvents = await dbContext.CurtailmentEvents
                .Where(e => e.Status == EventStatus.Completed && 
                           e.EndTime < DateTime.UtcNow && 
                           e.EndTime > DateTime.UtcNow.AddDays(-3)) // Look at events from the last 3 days
                .Include(e => e.Participations)
                .ToListAsync();

            foreach (var curtailmentEvent in completedEvents)
            {
                _logger.LogInformation("Processing completed event: {EventId} - {Title}", 
                    curtailmentEvent.Id, curtailmentEvent.Title);

                // Get the event creator's wallet
                var creatorWallet = await userWalletService.GetWalletByUserIdAsync(curtailmentEvent.CreatedBy);
                if (creatorWallet == null)
                {
                    _logger.LogWarning("Creator wallet not found for event {EventId}", curtailmentEvent.Id);
                    continue;
                }

                // Process each participant that hasn't been verified yet
                foreach (var participation in curtailmentEvent.Participations.Where(p => 
                    p.Status == ParticipationStatus.Registered || p.Status == ParticipationStatus.Participating))
                {
                    try
                    {
                        // Get participant wallet
                        var participantWallet = await userWalletService.GetWalletByUserIdAsync(participation.UserId);
                        if (participantWallet == null)
                        {
                            _logger.LogWarning("Participant wallet not found: {UserId}", participation.UserId);
                            continue;
                        }

                        // Get participant device data
                        var devices = await deviceService.GetDevicesByUserIdAsync(participation.UserId);
                        
                        // Calculate energy saved (in a real system, you would get this from IoT data or smart meter readings)
                        // This is a simplistic simulation - replace with your actual data source
                        decimal energySaved = CalculateEnergySaved(devices, curtailmentEvent.StartTime, curtailmentEvent.EndTime);
                        
                        // Determine if they met their target
                        // In a real system, you would compare against their baseline or commitment
                        bool metTarget = energySaved > 0;
                        
                        // Record the verified energy savings
                        await curtailmentService.RecordEnergySavedAsync(
                            curtailmentEvent.Id, 
                            participation.UserId, 
                            energySaved);
                        
                        // Calculate reward
                        decimal reward = energySaved * curtailmentEvent.RewardPerKwh;
                        
                        // Now create the appropriate XRPL transaction based on performance
                        if (metTarget)
                        {
                            // In a real system, you would retrieve the escrow sequence from your database
                            uint escrowSequence = 12345; // Placeholder
                            
                            // Create EscrowFinish payload
                            string condition = Convert.ToBase64String(
                                System.Text.Encoding.UTF8.GetBytes(
                                    $"Participant:{curtailmentEvent.Id}-{participation.UserId}"));
                            
                            string fulfillment = Convert.ToBase64String(
                                System.Text.Encoding.UTF8.GetBytes($"Fulfillment-for-{condition}"));
                            
                            var payloadJson = $"{{ \"TransactionType\": \"EscrowFinish\", " +
                                              $"\"Account\": \"{creatorWallet.Address}\", " +
                                              $"\"Owner\": \"{creatorWallet.Address}\", " +
                                              $"\"OfferSequence\": {escrowSequence}, " +
                                              $"\"Condition\": \"{condition}\", " +
                                              $"\"Fulfillment\": \"{fulfillment}\" }}";

                            // In a production system, you would use your system account to sign this
                            // Instead, we'll create a payload for manual signing by the operator
                            var payload = new XummPostJsonPayload(payloadJson);
                            payload.CustomMeta = new XummPayloadCustomMeta
                            {
                                Identifier = $"auto_reward_{curtailmentEvent.Id}_{participation.UserId}",
                                Instruction = $"Auto-release reward escrow for successful performance: {energySaved} kWh saved"
                            };

                            var xummResponse = await xummClient.CreateAsync(payload);
                            
                            // Mark reward as processed
                            await curtailmentService.VerifyParticipationAsync(
                                curtailmentEvent.Id, participation.UserId);
                            
                            await curtailmentService.MarkRewardClaimed(
                                curtailmentEvent.Id, participation.UserId, reward);
                            
                            await userWalletService.UpdateTotalRewardsClaimedAsync(
                                participation.UserId, reward);
                            
                            _logger.LogInformation(
                                "Processed successful participation for {UserId}: {EnergySaved} kWh, {Reward} XRP", 
                                participation.UserId, energySaved, reward);
                        }
                        else
                        {
                            // If they didn't meet the target, the escrow would be canceled
                            uint escrowSequence = 12345; // Placeholder
                            
                            var payloadJson = $"{{ \"TransactionType\": \"EscrowCancel\", " +
                                              $"\"Account\": \"{creatorWallet.Address}\", " +
                                              $"\"Owner\": \"{creatorWallet.Address}\", " +
                                              $"\"OfferSequence\": {escrowSequence} }}";

                            var payload = new XummPostJsonPayload(payloadJson);
                            payload.CustomMeta = new XummPayloadCustomMeta
                            {
                                Identifier = $"auto_cancel_{curtailmentEvent.Id}_{participation.UserId}",
                                Instruction = $"Auto-cancel reward escrow for unsuccessful performance: {energySaved} kWh saved"
                            };

                            var xummResponse = await xummClient.CreateAsync(payload);
                            
                            // Mark verification as processed but no reward
                            await curtailmentService.VerifyParticipationAsync(
                                curtailmentEvent.Id, participation.UserId);
                            
                            _logger.LogInformation(
                                "Processed unsuccessful participation for {UserId}: {EnergySaved} kWh, no reward", 
                                participation.UserId, energySaved);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing participation for user {UserId} in event {EventId}", 
                            participation.UserId, curtailmentEvent.Id);
                    }
                }

                // Update event total energy saved and rewards
                var totalEnergySaved = await curtailmentService.CalculateTotalEnergySavedAsync(curtailmentEvent.Id);
                var totalRewards = await curtailmentService.CalculateTotalRewardsForEventAsync(curtailmentEvent.Id);
                
                _logger.LogInformation("Event {EventId} completed processing: {TotalEnergySaved} kWh saved, {TotalRewards} XRP in rewards", 
                    curtailmentEvent.Id, totalEnergySaved, totalRewards);
            }
        }

        private decimal CalculateEnergySaved(IEnumerable<DeviceDto> devices, DateTime startTime, DateTime endTime)
        {
            // This is a placeholder implementation
            // In a real system, you would:
            // 1. Retrieve historical energy usage data for the devices during the event period
            // 2. Compare against a baseline (previous usage, predicted usage, etc.)
            // 3. Calculate the actual energy saved
            
            // For simulation, we'll generate a random energy saving between 0 and the total device capacity
            var random = new Random();
            decimal totalCapacity = devices.Sum(d => (decimal)d.EnergyCapacity);
            
            // Simulate a 0-80% reduction of total capacity
            return totalCapacity * (decimal)random.NextDouble() * 0.8m;
        }
    }
}