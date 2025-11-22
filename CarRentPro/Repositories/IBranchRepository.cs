using CarRentPro.Models;

namespace CarRentPro.Repositories
{
    public interface IBranchRepository
    {
        Task<List<Branch>> GetAllBranchesAsync();
        Task<Branch> GetBranchByIdAsync(int id);
        Task<Branch> CreateBranchAsync(Branch branch);
        Task<Branch> UpdateBranchAsync(Branch branch);
        Task<bool> DeleteBranchAsync(int id);
        Task<bool> BranchHasVehiclesAsync(int branchId);
    }
}