using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CarRentPro.Models;

namespace CarRentPro.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<VehicleStock> VehicleStocks { get; set; }

        public DbSet<BlacklistEntry> BlacklistEntries { get; set; }

        public DbSet<PredefinedProfilePicture> PredefinedProfilePictures { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.Entity<Rental>()
                .Property(r => r.TotalPrice)
                .HasPrecision(18, 2);

            builder.Entity<Vehicle>()
                .Property(v => v.PricePerDay)
                .HasPrecision(18, 2);

           
            builder.Entity<Rental>()
                .HasOne(r => r.User)
                .WithMany(u => u.Rentals)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Rental>()
                .HasOne(r => r.Vehicle)
                .WithMany(v => v.Rentals)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Branch)
                .WithMany(b => b.Vehicles)
                .HasForeignKey(v => v.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VehicleStock>()
                .HasOne(vs => vs.Branch)
                .WithMany(b => b.VehicleStocks)
                .HasForeignKey(vs => vs.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<VehicleStock>()
                .HasOne(vs => vs.Vehicle)
                .WithMany(v => v.VehicleStocks)
                .HasForeignKey(vs => vs.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BlacklistEntry>()
                .HasOne(b => b.User)
                .WithMany(u => u.BlacklistEntries)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

         
            builder.Entity<BlacklistEntry>()
                .HasIndex(b => new { b.UserId, b.IsActive, b.ExpirationDate })
                .HasDatabaseName("IX_Blacklist_ActiveCheck");

            builder.Entity<PredefinedProfilePicture>()
                .Property(p => p.ImageName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<PredefinedProfilePicture>()
                .Property(p => p.ImagePath)
                .IsRequired()
                .HasMaxLength(200);
        }
    }
}