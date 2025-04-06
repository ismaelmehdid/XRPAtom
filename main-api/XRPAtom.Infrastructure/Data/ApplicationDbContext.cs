using Microsoft.EntityFrameworkCore;
using XRPAtom.Core.Domain;

namespace XRPAtom.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<CurtailmentEvent> CurtailmentEvents { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<MarketplaceListing> MarketplaceListings { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<EventParticipation> EventParticipations { get; set; }
        public DbSet<MarketplaceTransaction> MarketplaceTransactions { get; set; }
        public DbSet<EscrowDetail> EscrowDetails { get; set; }
        public DbSet<RewardAllocation> RewardAllocations { get; set; }
        public DbSet<RewardPayment> RewardPayments { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure UserWallet - Fix the one-to-one relationship
            modelBuilder.Entity<UserWallet>()
                .HasOne(w => w.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<UserWallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Add precision to decimal properties in UserWallet
            modelBuilder.Entity<UserWallet>()
                .Property(w => w.Balance)
                .HasPrecision(18, 6);

            modelBuilder.Entity<UserWallet>()
                .Property(w => w.AtomTokenBalance)
                .HasPrecision(18, 6);

            modelBuilder.Entity<UserWallet>()
                .Property(w => w.TotalRewardsClaimed)
                .HasPrecision(18, 6);

            // Configure Device - Fix the relationship with User
            modelBuilder.Entity<Device>()
                .HasOne(d => d.User)
                .WithMany(u => u.Devices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CurtailmentEvent
            modelBuilder.Entity<CurtailmentEvent>()
                .Property(e => e.RewardPerKwh)
                .HasPrecision(18, 6);
                
            modelBuilder.Entity<CurtailmentEvent>()
                .Property(e => e.TotalEnergySaved)
                .HasPrecision(18, 6);
                
            modelBuilder.Entity<CurtailmentEvent>()
                .Property(e => e.TotalRewardsPaid)
                .HasPrecision(18, 6);
                
            modelBuilder.Entity<CurtailmentEvent>()
                .HasMany(e => e.Participations)
                .WithOne(p => p.Event)
                .HasForeignKey(p => p.EventId);

            // Configure EventParticipation (many-to-many between Events and Users)
            modelBuilder.Entity<EventParticipation>()
                .HasKey(ep => new { ep.EventId, ep.UserId });

            modelBuilder.Entity<EventParticipation>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.Participations)
                .HasForeignKey(ep => ep.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventParticipation>()
                .HasOne(ep => ep.User)
                .WithMany(u => u.EventParticipations)
                .HasForeignKey(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete conflicts

            modelBuilder.Entity<EventParticipation>()
                .Property(ep => ep.EnergySaved)
                .HasPrecision(18, 6);

            modelBuilder.Entity<EventParticipation>()
                .Property(ep => ep.RewardAmount)
                .HasPrecision(18, 6);

            // Configure Transaction
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TransactionHash)
                .IsUnique();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 6);

            // Configure MarketplaceListing
            modelBuilder.Entity<MarketplaceListing>()
                .Property(m => m.PricePerKwh)
                .HasPrecision(18, 6);

            modelBuilder.Entity<MarketplaceListing>()
                .Property(m => m.MinKwh)
                .HasPrecision(18, 6);

            modelBuilder.Entity<MarketplaceListing>()
                .Property(m => m.MaxKwh)
                .HasPrecision(18, 6);
                
            // Configure relationship with ProviderUser
            modelBuilder.Entity<MarketplaceListing>()
                .HasOne(m => m.ProviderUser)
                .WithMany()
                .HasForeignKey(m => m.Provider);

            // Configure MarketplaceTransaction
            modelBuilder.Entity<MarketplaceTransaction>()
                .Property(m => m.Amount)
                .HasPrecision(18, 6);

            modelBuilder.Entity<MarketplaceTransaction>()
                .Property(m => m.TotalPrice)
                .HasPrecision(18, 6);
                
            // Configure relationships for MarketplaceTransaction
            modelBuilder.Entity<MarketplaceTransaction>()
                .HasOne(m => m.Listing)
                .WithMany()
                .HasForeignKey(m => m.ListingId);
                
            modelBuilder.Entity<MarketplaceTransaction>()
                .HasOne(m => m.Buyer)
                .WithMany()
                .HasForeignKey(m => m.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<MarketplaceTransaction>()
                .HasOne(m => m.Seller)
                .WithMany()
                .HasForeignKey(m => m.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<EscrowDetail>()
                .HasIndex(e => e.EventId);
            
            modelBuilder.Entity<EscrowDetail>()
                .HasIndex(e => e.ParticipantId);
            
            modelBuilder.Entity<EscrowDetail>()
                .HasIndex(e => e.XummPayloadId);
            
            modelBuilder.Entity<EscrowDetail>()
                .HasIndex(e => e.TransactionHash);
        
            modelBuilder.Entity<EscrowDetail>()
                .HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<RewardAllocation>()
                .HasOne(ra => ra.Event)
                .WithMany()
                .HasForeignKey(ra => ra.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RewardAllocation>()
                .HasOne(ra => ra.Participant)
                .WithMany()
                .HasForeignKey(ra => ra.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RewardAllocation>()
                .Property(ra => ra.PotentialAmount)
                .HasPrecision(18, 6);

            modelBuilder.Entity<RewardAllocation>()
                .Property(ra => ra.ActualAmount)
                .HasPrecision(18, 6);

            modelBuilder.Entity<RewardPayment>()
                .HasOne(rp => rp.Event)
                .WithMany()
                .HasForeignKey(rp => rp.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RewardPayment>()
                .HasOne(rp => rp.Participant)
                .WithMany()
                .HasForeignKey(rp => rp.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RewardPayment>()
                .Property(rp => rp.Amount)
                .HasPrecision(18, 6);

            // Apply configurations from separate configuration classes
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}