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

        public async Task<Rental> GetRentalByIdAsync(int id)
        {
            return await _rentalRepository.GetRentalByIdAsync(id);
        }

        public async Task<List<Rental>> GetUserRentalsAsync(string userId)
        {
            return await _rentalRepository.GetUserRentalsAsync(userId);
        }

        public async Task<List<Rental>> GetAllRentalsAsync()
        {
            return await _rentalRepository.GetAllRentalsAsync();
        }

        public async Task<(bool Success, string Message, Rental Rental)> CreateRentalAsync(string userId, int vehicleId, DateTime returnDate)
        {
            try
            {
                
                if (await _rentalRepository.HasUserActiveRentalAsync(userId))
                {
                    return (false, "You already have an active rental. Please return your current vehicle before renting another one.", null);
                }

                
                if (!await _rentalRepository.IsVehicleAvailableAsync(vehicleId))
                {
                    return (false, "This vehicle is currently not available for rental.", null);
                }

                
                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null)
                {
                    return (false, "Vehicle not found.", null);
                }

                var rentalDate = DateTime.Now;
                var days = (returnDate - rentalDate).Days;
                if (days < 1) days = 1; 

                var totalPrice = days * vehicle.PricePerDay;

               
                var rental = new Rental
                {
                    UserId = userId,
                    VehicleId = vehicleId,
                    RentalDate = rentalDate,
                    ReturnDate = returnDate,
                    TotalPrice = totalPrice,
                    Status = "Active"
                };

                
                vehicle.IsAvailable = false;

                var createdRental = await _rentalRepository.CreateRentalAsync(rental);
                return (true, "Vehicle rented successfully!", createdRental);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating rental: {ex.Message}", null);
            }
        }

        public async Task<bool> CancelRentalAsync(int rentalId, string userId)
        {
            var rental = await _rentalRepository.GetRentalByIdAsync(rentalId);
            if (rental == null || rental.UserId != userId)
            {
                return false;
            }

            return await _rentalRepository.CancelRentalAsync(rentalId);
        }

        public async Task<bool> CanUserRentVehicleAsync(string userId, int vehicleId)
        {
            var hasActiveRental = await _rentalRepository.HasUserActiveRentalAsync(userId);
            var isVehicleAvailable = await _rentalRepository.IsVehicleAvailableAsync(vehicleId);

            return !hasActiveRental && isVehicleAvailable;
        }

        public async Task<bool> IsVehicleAvailableAsync(int vehicleId)
        {
            return await _rentalRepository.IsVehicleAvailableAsync(vehicleId);
        }
    }
}