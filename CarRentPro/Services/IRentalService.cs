using CarRentPro.Models;

namespace CarRentPro.Services
{
    public interface IRentalService
    {
        Task<Rental> GetRentalByIdAsync(int id);
        Task<List<Rental>> GetUserRentalsAsync(string userId);
        Task<List<Rental>> GetAllRentalsAsync();
        Task<(bool Success, string Message, Rental Rental)> CreateRentalAsync(string userId, int vehicleId, DateTime returnDate);
        Task<bool> CancelRentalAsync(int rentalId, string userId);
        Task<bool> CanUserRentVehicleAsync(string userId, int vehicleId);
        Task<bool> IsVehicleAvailableAsync(int vehicleId);
    }
}