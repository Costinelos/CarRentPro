using CarRentPro.Models;

namespace CarRentPro.Repositories
{
    public interface IRentalRepository
    {
        Task<bool> IsVehicleAvailableForDates(int vehicleId, DateTime startDate, DateTime endDate);
        Task<bool> HasUserActiveRental(string userId);
        Task<Rental> CreateRentalAsync(Rental rental);
        Task<List<Rental>> GetUserRentalsAsync(string userId);
        Task<List<Rental>> GetActiveRentalsAsync();
        Task<List<Rental>> GetAllRentalsAsync();
        Task<Rental> GetRentalByIdAsync(int id);
        Task<bool> UpdateRentalAsync(Rental rental);
        Task<bool> CancelRentalAsync(int rentalId);
        Task<List<Rental>> GetRentalsByVehicleIdAsync(int vehicleId);
        Task<List<Rental>> GetExpiredRentalsAsync();
        Task<bool> CompleteRentalAsync(int rentalId);
    }
}