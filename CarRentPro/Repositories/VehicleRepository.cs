using Microsoft.EntityFrameworkCore;
using CarRentPro.Models;
using System.Data;

namespace CarRentPro.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            return await _context.Vehicles.Include(v => v.Branch).ToListAsync();
        }

        public async Task<Vehicle> GetVehicleByIdAsync(int id)
        {
            return await _context.Vehicles.Include(v => v.Branch).FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _context.Vehicles.Include(v => v.Branch).Where(v => v.IsAvailable).ToListAsync();
        }

        public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
        {
            var existing = await _context.Vehicles.FindAsync(vehicle.Id);
            if (existing == null) throw new Exception("Vehicle not found");

            existing.Brand = vehicle.Brand;
            existing.Model = vehicle.Model;
            existing.Year = vehicle.Year;
            existing.Color = vehicle.Color;
            existing.PricePerDay = vehicle.PricePerDay;
            existing.Description = vehicle.Description;
            existing.ImageUrl = vehicle.ImageUrl;
            existing.BranchId = vehicle.BranchId;
            existing.IsAvailable = vehicle.IsAvailable;
            // IMPORTANT PENTRU MULTIMEDIA:
            existing.VideoUrl = vehicle.VideoUrl;

            _context.Vehicles.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return false;

            if (await HasActiveRentalsAsync(id))
            {
                vehicle.IsAvailable = false;
                _context.Update(vehicle);
            }
            else
            {
                _context.Vehicles.Remove(vehicle);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> HasActiveRentalsAsync(int vehicleId)
        {
            return await _context.Rentals.AnyAsync(r => r.VehicleId == vehicleId && r.Status == "Active");
        }

        public async Task<bool> HasAnyRentalsAsync(int vehicleId)
        {
            return await _context.Rentals.AnyAsync(r => r.VehicleId == vehicleId);
        }

        public async Task<bool> ForceDeleteVehicleAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Rentals WHERE VehicleId = {0}", id);
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM VehicleStocks WHERE VehicleId = {0}", id);
                var result = await _context.Database.ExecuteSqlRawAsync("DELETE FROM Vehicles WHERE Id = {0}", id);
                await transaction.CommitAsync();
                return result > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<Vehicle>> GetVehiclesByBranchAsync(int branchId)
        {
            return await _context.Vehicles.Where(v => v.BranchId == branchId).ToListAsync();
        }
    }
}