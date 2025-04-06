using System.ComponentModel.DataAnnotations;
using XRPAtom.Core.Domain;

namespace XRPAtom.Core.DTOs
{
    public class DeviceDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public DeviceStatus Status { get; set; }
        public bool Enrolled { get; set; }
        public int CurtailmentLevel { get; set; }
        public DateTime LastSeen { get; set; }
        public string Location { get; set; }
        public double EnergyCapacity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public object Preferences { get; set; } // Deserialized from JSON
    }

    public class CreateDeviceDto
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Manufacturer { get; set; }
        
        [StringLength(100)]
        public string Model { get; set; }
        
        public bool Enrolled { get; set; } = false;
        
        [Range(0, 3)]
        public int CurtailmentLevel { get; set; } = 0;
        
        [StringLength(200)]
        public string Location { get; set; }
        
        [Range(0, 10000)]
        public double EnergyCapacity { get; set; } = 0;
        
        public object Preferences { get; set; }
    }

    public class UpdateDeviceDto
    {
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(100)]
        public string? Model { get; set; }
        
        public bool? Enrolled { get; set; }
        
        [Range(0, 3)]
        public int? CurtailmentLevel { get; set; }
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        [Range(0, 100)]
        public double? EnergyCapacity { get; set; }
        
        public object? Preferences { get; set; }
        
        public DeviceStatus? Status { get; set; }
    }

    public class DeviceStatusUpdateDto
    {
        [Required]
        public DeviceStatus Status { get; set; }
    }

    public class DeviceEnrollmentDto
    {
        [Required]
        public bool Enrolled { get; set; }
    }

    public class DeviceCurtailmentLevelDto
    {
        [Required]
        [Range(0, 3)]
        public int CurtailmentLevel { get; set; }
    }

    public class DevicePreferencesDto
    {
        public object Preferences { get; set; }
    }
}