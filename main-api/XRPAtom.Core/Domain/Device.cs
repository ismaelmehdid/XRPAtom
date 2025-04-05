using System;
using System.ComponentModel.DataAnnotations;

namespace XRPAtom.Core.Domain
{
    public class Device
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
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
        
        [Required]
        public DeviceStatus Status { get; set; } = DeviceStatus.Offline;
        
        public bool Enrolled { get; set; } = false;
        
        public int CurtailmentLevel { get; set; } = 0;
        
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        
        [StringLength(200)]
        public string Location { get; set; }
        
        public double EnergyCapacity { get; set; } // in kWh
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // JSON serialized device-specific preferences
        [StringLength(2000)]
        public string Preferences { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
    }
    
    public enum DeviceStatus
    {
        Offline = 0,
        Online = 1,
        Curtailing = 2,
        Error = 3,
        Maintenance = 4
    }
}