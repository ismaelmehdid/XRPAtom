using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.Infrastructure.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(ApplicationDbContext context, ILogger<DeviceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DeviceDto> GetDeviceByIdAsync(string deviceId)
        {
            try
            {
                var device = await _context.Devices
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == deviceId);

                if (device == null)
                {
                    return null;
                }

                return MapToDeviceDto(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving device {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<IEnumerable<DeviceDto>> GetDevicesByUserIdAsync(string userId)
        {
            try
            {
                var devices = await _context.Devices
                    .Where(d => d.UserId == userId)
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                return devices.Select(MapToDeviceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving devices for user {UserId}", userId);
                throw;
            }
        }

        public async Task<DeviceDto> CreateDeviceAsync(CreateDeviceDto createDeviceDto)
        {
            try
            {
                var device = new Device
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = createDeviceDto.UserId,
                    Name = createDeviceDto.Name,
                    Type = createDeviceDto.Type,
                    Manufacturer = createDeviceDto.Manufacturer,
                    Model = createDeviceDto.Model,
                    Status = DeviceStatus.Offline,
                    Enrolled = createDeviceDto.Enrolled,
                    CurtailmentLevel = createDeviceDto.CurtailmentLevel,
                    Location = createDeviceDto.Location,
                    EnergyCapacity = createDeviceDto.EnergyCapacity,
                    CreatedAt = DateTime.UtcNow,
                    Preferences = createDeviceDto.Preferences != null 
                        ? System.Text.Json.JsonSerializer.Serialize(createDeviceDto.Preferences) 
                        : null
                };

                _context.Devices.Add(device);
                await _context.SaveChangesAsync();

                return MapToDeviceDto(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating device for user {UserId}", createDeviceDto.UserId);
                throw;
            }
        }

        public async Task<DeviceDto> UpdateDeviceAsync(string deviceId, UpdateDeviceDto updateDeviceDto)
        {
            try
            {
                var device = await _context.Devices.FindAsync(deviceId);
                if (device == null)
                {
                    return null;
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(updateDeviceDto.Name))
                {
                    device.Name = updateDeviceDto.Name;
                }

                if (!string.IsNullOrEmpty(updateDeviceDto.Model))
                {
                    device.Model = updateDeviceDto.Model;
                }

                if (!string.IsNullOrEmpty(updateDeviceDto.Location))
                {
                    device.Location = updateDeviceDto.Location;
                }

                if (updateDeviceDto.Enrolled.HasValue)
                {
                    device.Enrolled = updateDeviceDto.Enrolled.Value;
                }

                if (updateDeviceDto.CurtailmentLevel.HasValue)
                {
                    device.CurtailmentLevel = updateDeviceDto.CurtailmentLevel.Value;
                }

                if (updateDeviceDto.EnergyCapacity.HasValue)
                {
                    device.EnergyCapacity = updateDeviceDto.EnergyCapacity.Value;
                }

                if (updateDeviceDto.Preferences != null)
                {
                    device.Preferences = System.Text.Json.JsonSerializer.Serialize(updateDeviceDto.Preferences);
                }

                device.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return MapToDeviceDto(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<bool> DeleteDeviceAsync(string deviceId)
        {
            try
            {
                var device = await _context.Devices.FindAsync(deviceId);
                if (device == null)
                {
                    return false;
                }

                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting device {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<bool> UpdateDeviceStatusAsync(string deviceId, DeviceStatus status)
        {
            try
            {
                var device = await _context.Devices.FindAsync(deviceId);
                if (device == null)
                {
                    return false;
                }

                device.Status = status;
                device.LastSeen = DateTime.UtcNow;
                device.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for device {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<bool> EnrollDeviceAsync(string deviceId, bool enrolled)
        {
            try
            {
                var device = await _context.Devices.FindAsync(deviceId);
                if (device == null)
                {
                    return false;
                }

                device.Enrolled = enrolled;
                device.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating enrollment for device {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<bool> UpdateCurtailmentLevelAsync(string deviceId, int curtailmentLevel)
        {
            try
            {
                var device = await _context.Devices.FindAsync(deviceId);
                if (device == null)
                {
                    return false;
                }

                // Ensure curtailment level is between 0 and 3
                curtailmentLevel = Math.Max(0, Math.Min(3, curtailmentLevel));

                device.CurtailmentLevel = curtailmentLevel;
                device.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating curtailment level for device {DeviceId}", deviceId);
                throw;
            }
        }

        public async Task<bool> HasEnrolledDevices(string userId)
        {
            try
            {
                return await _context.Devices.AnyAsync(d => d.UserId == userId && d.Enrolled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} has enrolled devices", userId);
                throw;
            }
        }

        public async Task<int> GetEnrolledDeviceCountAsync(string userId)
        {
            try
            {
                return await _context.Devices.CountAsync(d => d.UserId == userId && d.Enrolled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrolled device count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<DeviceDto>> GetEligibleDevicesForCurtailmentAsync(string userId)
        {
            try
            {
                var devices = await _context.Devices
                    .Where(d => 
                        d.UserId == userId && 
                        d.Enrolled && 
                        d.Status == DeviceStatus.Online && 
                        d.CurtailmentLevel > 0)
                    .ToListAsync();

                return devices.Select(MapToDeviceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting eligible devices for curtailment for user {UserId}", userId);
                throw;
            }
        }

        public async Task<double> GetTotalCurtailmentCapacityAsync(string userId)
        {
            try
            {
                // Calculate the total capacity based on energy capacity and curtailment level
                var devices = await _context.Devices
                    .Where(d => d.UserId == userId && d.Enrolled && d.Status == DeviceStatus.Online)
                    .ToListAsync();

                double totalCapacity = 0;
                foreach (var device in devices)
                {
                    // Simple formula: capacity * (curtailmentLevel / 3.0)
                    // Level 0 = 0% capacity
                    // Level 1 = 33% capacity
                    // Level 2 = 66% capacity
                    // Level 3 = 100% capacity
                    totalCapacity += device.EnergyCapacity * (device.CurtailmentLevel / 3.0);
                }

                return totalCapacity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total curtailment capacity for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Dictionary<string, List<DeviceDto>>> GetDevicesByTypeAsync(string userId)
        {
            try
            {
                var devices = await _context.Devices
                    .Where(d => d.UserId == userId)
                    .ToListAsync();

                var result = new Dictionary<string, List<DeviceDto>>();
                
                // Group devices by type
                foreach (var device in devices)
                {
                    if (!result.ContainsKey(device.Type))
                    {
                        result[device.Type] = new List<DeviceDto>();
                    }
                    
                    result[device.Type].Add(MapToDeviceDto(device));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting devices by type for user {UserId}", userId);
                throw;
            }
        }

        private DeviceDto MapToDeviceDto(Device device)
        {
            if (device == null) return null;

            object preferences = null;
            if (!string.IsNullOrEmpty(device.Preferences))
            {
                try
                {
                    preferences = System.Text.Json.JsonSerializer.Deserialize<object>(device.Preferences);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing preferences for device {DeviceId}", device.Id);
                }
            }

            return new DeviceDto
            {
                Id = device.Id,
                UserId = device.UserId,
                Name = device.Name,
                Type = device.Type,
                Manufacturer = device.Manufacturer,
                Model = device.Model,
                Status = device.Status,
                Enrolled = device.Enrolled,
                CurtailmentLevel = device.CurtailmentLevel,
                LastSeen = device.LastSeen,
                Location = device.Location,
                EnergyCapacity = device.EnergyCapacity,
                CreatedAt = device.CreatedAt,
                UpdatedAt = device.UpdatedAt,
                Preferences = preferences
            };
        }
    }
}