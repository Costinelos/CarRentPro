using CarRentPro.Models;

namespace CarRentPro.Services
{
    public interface IVehicleService
    {
        Task<List<Vehicle>> GetAllVehiclesAsync();
        Task<Vehicle> GetVehicleByIdAsync(int id);
        Task<List<Vehicle>> GetAvailableVehiclesAsync();
        Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
        Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle);
        Task<bool> DeleteVehicleAsync(int id);
        Task<List<Branch>> GetAllBranchesAsync();
    }
}