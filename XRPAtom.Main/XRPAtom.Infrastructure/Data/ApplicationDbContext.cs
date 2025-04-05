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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure UserWallet
            modelBuilder.Entity<UserWallet>()
                .HasIndex(w => w.Address)
                .IsUnique();

            modelBuilder.Entity<UserWallet>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<UserWallet>(w => w.UserId)
                .IsRequired();

            // Configure Device
            modelBuilder.Entity<Device>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CurtailmentEvent
            modelBuilder.Entity<CurtailmentEvent>()
                .Property(e => e.RewardPerKwh)
                .HasPrecision(18, 6);

            // Configure EventParticipation (many-to-many between Events and Users)
            modelBuilder.Entity<EventParticipation>()
                .HasKey(ep => new { ep.EventId, ep.UserId });

            modelBuilder.Entity<EventParticipation>()
                .HasOne<CurtailmentEvent>()
                .WithMany()
                .HasForeignKey(ep => ep.EventId);

            modelBuilder.Entity<EventParticipation>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(ep => ep.UserId);

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

            // Configure MarketplaceTransaction
            modelBuilder.Entity<MarketplaceTransaction>()
                .Property(m => m.Amount)
                .HasPrecision(18, 6);

            modelBuilder.Entity<MarketplaceTransaction>()
                .Property(m => m.TotalPrice)
                .HasPrecision(18, 6);

            // Apply configurations from separate configuration classes
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}