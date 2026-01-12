using CarRentPro.Models;
using CarRentPro.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CarRentPro.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ApplicationDbContext _context;

        public VehicleService(IVehicleRepository vehicleRepository, ApplicationDbContext context)
        {
            _vehicleRepository = vehicleRepository;
            _context = context;
        }

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            return await _vehicleRepository.GetAllVehiclesAsync();
        }

        public async Task<Vehicle> GetVehicleByIdAsync(int id)
        {
            return await _vehicleRepository.GetVehicleByIdAsync(id);
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _vehicleRepository.GetAvailableVehiclesAsync();
        }

        public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
        {
            return await _vehicleRepository.CreateVehicleAsync(vehicle);
        }

        public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
        {
            return await _vehicleRepository.UpdateVehicleAsync(vehicle);
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            return await _vehicleRepository.DeleteVehicleAsync(id);
        }

        public async Task<List<Branch>> GetAllBranchesAsync()
        {
            return await _context.Branches.ToListAsync();
        }

        public async Task<bool> HasActiveRentalsAsync(int vehicleId)
        {
            return await _vehicleRepository.HasActiveRentalsAsync(vehicleId);
        }

      
        public async Task<bool> HasAnyRentalsAsync(int vehicleId)
        {
            return await _vehicleRepository.HasAnyRentalsAsync(vehicleId);
        }
    }
}