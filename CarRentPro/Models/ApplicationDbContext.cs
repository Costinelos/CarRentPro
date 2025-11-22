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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurare pentru proprietățile decimal
            builder.Entity<Rental>()
                .Property(r => r.TotalPrice)
                .HasPrecision(18, 2);

            builder.Entity<Vehicle>()
                .Property(v => v.PricePerDay)
                .HasPrecision(18, 2);

            // Configurări pentru relații și constrângeri
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
        }
    }
}