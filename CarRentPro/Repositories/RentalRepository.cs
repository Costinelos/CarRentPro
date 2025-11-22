using Microsoft.EntityFrameworkCore;
using CarRentPro.Models;

namespace CarRentPro.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly ApplicationDbContext _context;

        public RentalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Rental> GetRentalByIdAsync(int id)
        {
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .ThenInclude(v => v.Branch)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Rental>> GetUserRentalsAsync(string userId)
        {
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .ThenInclude(v => v.Branch)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RentalDate)
                .ToListAsync();
        }

        public async Task<List<Rental>> GetAllRentalsAsync()
        {
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .ThenInclude(v => v.Branch)
                .Include(r => r.User)
                .OrderByDescending(r => r.RentalDate)
                .ToListAsync();
        }

        public async Task<List<Rental>> GetActiveRentalsAsync()
        {
            return await _context.Rentals
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .Where(r => r.Status == "Active" && r.ReturnDate >= DateTime.Now)
                .ToListAsync();
        }

        public async Task<bool> IsVehicleAvailableAsync(int vehicleId)
        {
            
            var activeRentals = await _context.Rentals
                .Where(r => r.VehicleId == vehicleId && r.Status == "Active" && r.ReturnDate >= DateTime.Now)
                .ToListAsync();

            return !activeRentals.Any();
        }

        public async Task<bool> HasUserActiveRentalAsync(string userId)
        {
            
            var activeRentals = await _context.Rentals
                .Where(r => r.UserId == userId && r.Status == "Active" && r.ReturnDate >= DateTime.Now)
                .ToListAsync();

            return activeRentals.Any();
        }

        public async Task<Rental> CreateRentalAsync(Rental rental)
        {
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();
            return rental;
        }

        public async Task<bool> UpdateRentalAsync(Rental rental)
        {
            _context.Rentals.Update(rental);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> CancelRentalAsync(int rentalId)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null) return false;

            rental.Status = "Cancelled";
            rental.Vehicle.IsAvailable = true; 

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> CompleteRentalAsync(int rentalId)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null) return false;

            rental.Status = "Completed";
            rental.Vehicle.IsAvailable = true; 

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}