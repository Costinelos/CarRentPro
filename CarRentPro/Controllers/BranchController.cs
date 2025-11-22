using Microsoft.AspNetCore.Mvc;
using CarRentPro.Models;
using CarRentPro.Services;

namespace CarRentPro.Controllers
{
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        // GET: Branch/Index
        public async Task<IActionResult> Index()
        {
            var branches = await _branchService.GetAllBranchesAsync();
            return View(branches);
        }

        // GET: Branch/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Branch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch branch)
        {
            if (ModelState.IsValid)
            {
                await _branchService.CreateBranchAsync(branch);
                TempData["SuccessMessage"] = "Branch added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        // GET: Branch/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branch = await _branchService.GetBranchByIdAsync(id.Value);
            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        // POST: Branch/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Branch branch)
        {
            if (id != branch.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _branchService.UpdateBranchAsync(branch);
                    TempData["SuccessMessage"] = "Branch updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error updating branch: " + ex.Message;
                    return View(branch);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        // GET: Branch/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branch = await _branchService.GetBranchByIdAsync(id.Value);
            if (branch == null)
            {
                return NotFound();
            }

            
            var hasVehicles = await _branchService.BranchHasVehiclesAsync(id.Value);
            if (hasVehicles)
            {
                TempData["ErrorMessage"] = "Cannot delete branch that has vehicles assigned. Please reassign or delete the vehicles first.";
                return RedirectToAction(nameof(Index));
            }

            return View(branch);
        }

        // POST: Branch/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _branchService.DeleteBranchAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Branch deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Branch not found!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}