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
            if (existing == null)
            {
                throw new Exception("Vehicle not found");
            }

            existing.Brand = vehicle.Brand;
            existing.Model = vehicle.Model;
            existing.Year = vehicle.Year;
            existing.Color = vehicle.Color;
            existing.PricePerDay = vehicle.PricePerDay;
            existing.Description = vehicle.Description;
            existing.ImageUrl = vehicle.ImageUrl;
            existing.BranchId = vehicle.BranchId;
            existing.IsAvailable = vehicle.IsAvailable;
            
            // IMPORTANT PENTRU MULTIMEDIA (Păstrat din branch-ul AI)
            existing.VideoUrl = vehicle.VideoUrl;

            _context.Vehicles.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                bool hasActiveRentals = await HasActiveRentalsAsync(id);

                if (hasActiveRentals)
                {
                    // Dacă are închirieri active, doar îl marcăm ca indisponibil
                    vehicle.IsAvailable = false;
                    _context.Vehicles.Update(vehicle);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return false; 
                }

                // Ștergem stocul asociat
                var vehicleStocks = await _context.VehicleStocks
                    .Where(vs => vs.VehicleId == id)
                    .ToListAsync();

                if (vehicleStocks.Any())
                {
                    _context.VehicleStocks.RemoveRange(vehicleStocks);
                }

                // Ștergem închirierile (care nu sunt active, ex: istoricul) dacă logica business permite
                var rentals = await _context.Rentals
                    .Where(r => r.VehicleId == id)
                    .ToListAsync();

                if (rentals.Any())
                {
                    _context.Rentals.RemoveRange(rentals);
                }

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"DeleteVehicleAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Vehicle>> GetVehiclesByBranchAsync(int branchId)
        {
            return await _context.Vehicles.Where(v => v.BranchId == branchId).ToListAsync();
        }

        public async Task<bool> HasActiveRentalsAsync(int vehicleId)
        {
            var now = DateTime.Now;

            // Verificăm închirierile active după Status sau Dată
            return await _context.Rentals.AnyAsync(r => r.VehicleId == vehicleId && 
                (r.Status == "Active" || 
                 r.RentalDate > now || 
                 (r.RentalDate <= now && (r.ReturnDate == null || r.ReturnDate > now))));
        }

        public async Task<bool> HasAnyRentalsAsync(int vehicleId)
        {
            return await _context.Rentals.AnyAsync(r => r.VehicleId ==