using CarRentPro.Models;

namespace CarRentPro.Repositories
{
    public interface IRentalRepository
    {
        Task<Rental> GetRentalByIdAsync(int id);
        Task<List<Rental>> GetUserRentalsAsync(string userId);
        Task<List<Rental>> GetAllRentalsAsync();
        Task<List<Rental>> GetActiveRentalsAsync();
        Task<bool> IsVehicleAvailableAsync(int vehicleId);
        Task<bool> HasUserActiveRentalAsync(string userId);
        Task<Rental> CreateRentalAsync(Rental rental);
        Task<bool> UpdateRentalAsync(Rental rental);
        Task<bool> CancelRentalAsync(int rentalId);
        Task<bool> CompleteRentalAsync(int rentalId);
    }
}