using CarRentPro.Models;

namespace CarRentPro.Repositories
{
    public interface IVehicleRepository
    {
        Task<List<Vehicle>> GetAllVehiclesAsync();
        Task<Vehicle> GetVehicleByIdAsync(int id);
        Task<List<Vehicle>> GetAvailableVehiclesAsync();
        Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
        Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle);
        Task<bool> DeleteVehicleAsync(int id);
        Task<List<Vehicle>> GetVehiclesByBranchAsync(int branchId);
        Task<bool> HasActiveRentalsAsync(int vehicleId);
        Task<bool> HasAnyRentalsAsync(int vehicleId);
        Task<bool> ForceDeleteVehicleAsync(int id);
    }
}