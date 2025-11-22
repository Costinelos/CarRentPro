using CarRentPro.Models;
using CarRentPro.Repositories;

namespace CarRentPro.Services
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _branchRepository;

        public BranchService(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<List<Branch>> GetAllBranchesAsync()
        {
            return await _branchRepository.GetAllBranchesAsync();
        }

        public async Task<Branch> GetBranchByIdAsync(int id)
        {
            return await _branchRepository.GetBranchByIdAsync(id);
        }

        public async Task<Branch> CreateBranchAsync(Branch branch)
        {
            return await _branchRepository.CreateBranchAsync(branch);
        }

        public async Task<Branch> UpdateBranchAsync(Branch branch)
        {
            return await _branchRepository.UpdateBranchAsync(branch);
        }

        public async Task<bool> DeleteBranchAsync(int id)
        {
            return await _branchRepository.DeleteBranchAsync(id);
        }

        public async Task<bool> BranchHasVehiclesAsync(int branchId)
        {
            return await _branchRepository.BranchHasVehiclesAsync(branchId);
        }
    }
}