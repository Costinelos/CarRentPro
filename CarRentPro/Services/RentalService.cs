using CarRentPro.Models;
using CarRentPro.Repositories;

namespace CarRentPro.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly ApplicationDbContext _context;

        public RentalService(IRentalRepository rentalRepository, ApplicationDbContext context)
        {
            _rentalRepository = rentalRepository;
            _context = context;
        }

        public async Task<(bool Success, string Message)> CreateRentalAsync(string userId, int vehicleId, DateTime startDate, DateTime endDate)
        {
            
            if (await _rentalRepository.HasUserActiveRental(userId))
            {
                return (false, "You already have an active rental. Please return your current vehicle before renting another one.");
            }

            
            if (!await _rentalRepository.IsVehicleAvailableForDates(vehicleId, startDate, endDate))
            {
                return (false, "This vehicle is not available for the selected dates. Please choose different dates or another vehicle.");
            }

            
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return (false, "Vehicle not found.");
            }

            if (!vehicle.IsAvailable)
            {
                return (false, "This vehicle is currently not available for rental.");
            }

            var days = (endDate - startDate).Days;
            var totalPrice = days * vehicle.PricePerDay;

            
            var rental = new Rental
            {
                UserId = userId,
                VehicleId = vehicleId,
                RentalDate = startDate,
                ReturnDate = endDate,
                TotalPrice = totalPrice,
                Status = "Active"
            };

           
            vehicle.IsAvailable = false;

            
            await _rentalRepository.CreateRentalAsync(rental);
            await _context.SaveChangesAsync();

            return (true, "Rental created successfully!");
        }

        public async Task<List<Rental>> GetUserRentalsAsync(string userId)
        {
            return await _rentalRepository.GetUserRentalsAsync(userId);
        }

        public async Task<bool> CanUserRentVehicle(string userId, int vehicleId, DateTime startDate, DateTime endDate)
        {
            
            var hasActiveRental = await _rentalRepository.HasUserActiveRental(userId);
            var isVehicleAvailable = await _rentalRepository.IsVehicleAvailableForDates(vehicleId, startDate, endDate);

            return !hasActiveRental && isVehicleAvailable;
        }
    }
}