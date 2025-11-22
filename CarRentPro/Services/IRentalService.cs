using CarRentPro.Models;

namespace CarRentPro.Services
{
    public interface IRentalService
    {
        Task<(bool Success, string Message)> CreateRentalAsync(string userId, int vehicleId, DateTime startDate, DateTime endDate);
        Task<List<Rental>> GetUserRentalsAsync(string userId);
        Task<bool> CanUserRentVehicle(string userId, int vehicleId, DateTime startDate, DateTime endDate);
    }
}