using Microsoft.EntityFrameworkCore;
using CarRentPro.Models;

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
            return await _context.Vehicles
                .Include(v => v.Branch)
                .ToListAsync();
        }

        public async Task<Vehicle> GetVehicleByIdAsync(int id)
        {
            return await _context.Vehicles
                .Include(v => v.Branch)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Branch)
                .Where(v => v.IsAvailable)
                .ToListAsync();
        }

        public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
        {
            // Găsește vehiculul existent
            var existingVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
            if (existingVehicle == null)
            {
                throw new Exception("Vehicle not found");
            }

            // Actualizează proprietățile
            existingVehicle.Brand = vehicle.Brand;
            existingVehicle.Model = vehicle.Model;
            existingVehicle.Year = vehicle.Year;
            existingVehicle.Color = vehicle.Color;
            existingVehicle.PricePerDay = vehicle.PricePerDay;
            existingVehicle.Description = vehicle.Description;
            existingVehicle.ImageUrl = vehicle.ImageUrl;
            existingVehicle.BranchId = vehicle.BranchId;
            existingVehicle.IsAvailable = vehicle.IsAvailable;

            _context.Vehicles.Update(existingVehicle);
            await _context.SaveChangesAsync();
            return existingVehicle;
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
                return false;

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Vehicle>> GetVehiclesByBranchAsync(int branchId)
        {
            return await _context.Vehicles
                .Include(v => v.Branch)
                .Where(v => v.BranchId == branchId)
                .ToListAsync();
        }
    }
}