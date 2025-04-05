using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XRPAtom.Core.Domain;
using XRPAtom.Core.Repositories;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.Infrastructure.Data.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly ApplicationDbContext _context;

        public DeviceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Device> GetByIdAsync(string deviceId)
        {
            return await _context.Devices
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == deviceId);
        }

        public async Task<IEnumerable<Device>> GetDevicesByUserIdAsync(string userId)
        {
            return await _context.Devices
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Device> CreateAsync(Device device)
        {
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<Device> UpdateAsync(Device device)
        {
            var existingDevice = await _context.Devices.FindAsync(device.Id);
            
            if (existingDevice == null)
            {
                throw new InvalidOperationException("Device not found.");
            }

            // Update mutable properties
            existingDevice.Name = device.Name ?? existingDevice.Name;
            existingDevice.Model = device.Model ?? existingDevice.Model;
            existingDevice.Location = device.Location ?? existingDevice.Location;
            existingDevice.Enrolled = device.Enrolled;
            existingDevice.CurtailmentLevel = device.CurtailmentLevel;
            existingDevice.Status = device.Status;
            existingDevice.Preferences = device.Preferences ?? existingDevice.Preferences;
            existingDevice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingDevice;
        }

        public async Task<bool> DeleteAsync(string deviceId)
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

        public async Task<bool> UpdateStatusAsync(string deviceId, DeviceStatus status)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            
            if (device == null)
            {
                return false;
            }

            device.Status = status;
            device.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEnrollmentAsync(string deviceId, bool enrolled)
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

        public async Task<bool> UpdateCurtailmentLevelAsync(string deviceId, int curtailmentLevel)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            
            if (device == null)
            {
                return false;
            }

            device.CurtailmentLevel = curtailmentLevel;
            device.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetEnrolledDeviceCountAsync(string userId)
        {
            return await _context.Devices
                .CountAsync(d => d.UserId == userId && d.Enrolled);
        }

        public async Task<IEnumerable<Device>> GetEligibleDevicesForCurtailmentAsync(string userId)
        {
            return await _context.Devices
                .Where(d => 
                    d.UserId == userId && 
                    d.Enrolled && 
                    d.Status == DeviceStatus.Online && 
                    d.CurtailmentLevel > 0)
                .ToListAsync();
        }

        public async Task<double> GetTotalCurtailmentCapacityAsync(string userId)
        {
            return await _context.Devices
                .Where(d => 
                    d.UserId == userId && 
                    d.Enrolled && 
                    d.Status == DeviceStatus.Online)
                .SumAsync(d => d.EnergyCapacity * (d.CurtailmentLevel / 3.0)); // Assuming 3 levels
        }

        public async Task<Dictionary<string, List<Device>>> GetDevicesByTypeAsync(string userId)
        {
            var devices = await _context.Devices
                .Where(d => d.UserId == userId)
                .ToListAsync();

            return devices
                .GroupBy(d => d.Type)
                .ToDictionary(
                    g => g.Key, 
                    g => g.ToList()
                );
        }
    }
}