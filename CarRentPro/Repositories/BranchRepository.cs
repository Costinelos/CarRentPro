using Microsoft.EntityFrameworkCore;
using CarRentPro.Models;

namespace CarRentPro.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly ApplicationDbContext _context;

        public BranchRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Branch>> GetAllBranchesAsync()
        {
            return await _context.Branches
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<Branch> GetBranchByIdAsync(int id)
        {
            return await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Branch> CreateBranchAsync(Branch branch)
        {
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            return branch;
        }

        public async Task<Branch> UpdateBranchAsync(Branch branch)
        {
           
            var existingBranch = await _context.Branches.FindAsync(branch.Id);
            if (existingBranch == null)
            {
                throw new Exception("Branch not found");
            }

            
            existingBranch.Name = branch.Name;
            existingBranch.Address = branch.Address;
            existingBranch.PhoneNumber = branch.PhoneNumber;

            _context.Branches.Update(existingBranch);
            await _context.SaveChangesAsync();
            return existingBranch;
        }

        public async Task<bool> DeleteBranchAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
                return false;

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BranchHasVehiclesAsync(int branchId)
        {
            return await _context.Vehicles
                .AnyAsync(v => v.BranchId == branchId);
        }
    }
}