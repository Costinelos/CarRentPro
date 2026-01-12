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
            var existingVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
            if (existingVehicle == null)
            {
                throw new Exception("Vehicle not found");
            }

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
                    vehicle.IsAvailable = false;
                    _context.Vehicles.Update(vehicle);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return false; 
                }
                var vehicleStocks = await _context.VehicleStocks
                    .Where(vs => vs.VehicleId == id)
                    .ToListAsync();

                if (vehicleStocks.Any())
                {
                    _context.VehicleStocks.RemoveRange(vehicleStocks);
                    await _context.SaveChangesAsync();
                }

               
                var rentals = await _context.Rentals
                    .Where(r => r.VehicleId == id)
                    .ToListAsync();

                if (rentals.Any())
                {
                    
                    var now = DateTime.Now;
                    var activeRentals = rentals.Where(r =>
                        r.RentalDate > now ||
                        (r.RentalDate <= now && (r.ReturnDate == null || r.ReturnDate > now)) ||
                        r.Status == "Active"
                    ).ToList();

                    if (activeRentals.Any())
                    {
                       
                        await transaction.RollbackAsync();
                        vehicle.IsAvailable = false;
                        _context.Vehicles.Update(vehicle);
                        await _context.SaveChangesAsync();
                        return false;
                    }

                 
                    _context.Rentals.RemoveRange(rentals);
                    await _context.SaveChangesAsync();
                }

               
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

            
                await transaction.CommitAsync();
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();

              
                try
                {
                    var vehicle = await _context.Vehicles.FindAsync(id);
                    if (vehicle != null)
                    {
                        vehicle.IsAvailable = false;
                        _context.Vehicles.Update(vehicle);
                        await _context.SaveChangesAsync();
                    }
                }
                catch
                {
                }

                Console.WriteLine($"DeleteVehicleAsync DB error: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                }

                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"DeleteVehicleAsync unexpected error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Vehicle>> GetVehiclesByBranchAsync(int branchId)
        {
            return await _context.Vehicles
                .Include(v => v.Branch)
                .Where(v => v.BranchId == branchId)
                .ToListAsync();
        }

        public async Task<bool> HasActiveRentalsAsync(int vehicleId)
        {
            var now = DateTime.Now;

            var rentals = await _context.Rentals
                .Where(r => r.VehicleId == vehicleId)
                .ToListAsync();

            if (!rentals.Any())
                return false;

            foreach (var rental in rentals)
            {
                if (rental.RentalDate > now)
                    return true;

                if (rental.RentalDate <= now)
                {
                    if (rental.ReturnDate == null) 
                        return true;

                    if (rental.ReturnDate > now) 
                        return true;
                }

               
                if (rental.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public async Task<bool> HasAnyRentalsAsync(int vehicleId)
        {
            return await _context.Rentals
                .AnyAsync(r => r.VehicleId == vehicleId);
        }

        
        public async Task<bool> ForceDeleteVehicleAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM Rentals WHERE VehicleId = {0}", id);

                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM VehicleStocks WHERE VehicleId = {0}", id);

                var result = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM Vehicles WHERE Id = {0}", id);

                await transaction.CommitAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"ForceDeleteVehicleAsync error: {ex.Message}");
                return false;
            }
        }
    }
}