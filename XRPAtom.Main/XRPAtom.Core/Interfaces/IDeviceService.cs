using System.Collections.Generic;
using System.Threading.Tasks;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;

namespace XRPAtom.Core.Interfaces
{
    public interface IDeviceService
    {
        Task<DeviceDto> GetDeviceByIdAsync(string deviceId);
        
        Task<IEnumerable<DeviceDto>> GetDevicesByUserIdAsync(string userId);
        
        Task<DeviceDto> CreateDeviceAsync(CreateDeviceDto createDeviceDto);
        
        Task<DeviceDto> UpdateDeviceAsync(string deviceId, UpdateDeviceDto updateDeviceDto);
        
        Task<bool> DeleteDeviceAsync(string deviceId);
        
        Task<bool> UpdateDeviceStatusAsync(string deviceId, DeviceStatus status);
        
        Task<bool> EnrollDeviceAsync(string deviceId, bool enrolled);
        
        Task<bool> UpdateCurtailmentLevelAsync(string deviceId, int curtailmentLevel);
        
        Task<bool> HasEnrolledDevices(string userId);
        
        Task<int> GetEnrolledDeviceCountAsync(string userId);
        
        Task<IEnumerable<DeviceDto>> GetEligibleDevicesForCurtailmentAsync(string userId);
        
        Task<double> GetTotalCurtailmentCapacityAsync(string userId);
        
        Task<Dictionary<string, List<DeviceDto>>> GetDevicesByTypeAsync(string userId);
    }
}