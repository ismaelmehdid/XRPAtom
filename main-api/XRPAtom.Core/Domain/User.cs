using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace XRPAtom.Core.Domain
{
    public class User : IdentityUser<string>
    {
        [PersonalData]
        public string Name { get; set; }

        [PersonalData]
        public UserRole Role { get; set; }

        [PersonalData]
        public string Organization { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
        public virtual UserWallet Wallet { get; set; }
        public virtual ICollection<EventParticipation> EventParticipations { get; set; } = new List<EventParticipation>();
    }
}