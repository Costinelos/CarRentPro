using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarRentPro.Models;
using CarRentPro.Services;

namespace CarRentPro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        public async Task<IActionResult> Index()
        {
            var branches = await _branchService.GetAllBranchesAsync();
            return View(branches);
        }

        public IActionResult Create()
        {
            return View();
        }

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