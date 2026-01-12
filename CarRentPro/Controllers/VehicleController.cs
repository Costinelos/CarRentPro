using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using CarRentPro.Models;
using CarRentPro.Services;
using CarRentPro.Repositories;

namespace CarRentPro.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class VehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<VehicleController> _logger;
        private readonly IWebHostEnvironment _environment;

        public VehicleController(
            IVehicleService vehicleService,
            IVehicleRepository vehicleRepository,
            ILogger<VehicleController> logger,
            IWebHostEnvironment environment)
        {
            _vehicleService = vehicleService;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return View(vehicles);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        public async Task<IActionResult> Create()
        {
            await LoadBranchesViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle, IFormFile? imageFile)
        {
            _logger.LogInformation("=== VEHICLE CREATE START ===");
            _logger.LogInformation($"ModelState IsValid: {ModelState.IsValid}");

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        vehicle.ImageUrl = await SaveImage(imageFile);
                    }

                    _logger.LogInformation($"Creating vehicle: {vehicle.Brand} {vehicle.Model}, BranchId: {vehicle.BranchId}");

                    var branches = await _vehicleService.GetAllBranchesAsync();
                    var selectedBranch = branches.FirstOrDefault(b => b.Id == vehicle.BranchId);

                    if (selectedBranch == null)
                    {
                        TempData["ErrorMessage"] = "Selected branch does not exist!";
                        await LoadBranchesViewBag();
                        return View(vehicle);
                    }

                    await _vehicleService.CreateVehicleAsync(vehicle);
                    TempData["SuccessMessage"] = "Vehicle added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating vehicle");
                    TempData["ErrorMessage"] = "Error creating vehicle: " + ex.Message;
                }
            }
            else
            {
                _logger.LogWarning("ModelState is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }
                TempData["ErrorMessage"] = "Please correct the validation errors.";
            }

            await LoadBranchesViewBag();
            return View(vehicle);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null)
            {
                return NotFound();
            }

            await LoadBranchesViewBag();
            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehicle vehicle, IFormFile? imageFile)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile == null || imageFile.Length == 0)
                    {
                        var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
                        if (existingVehicle != null)
                        {
                            vehicle.ImageUrl = existingVehicle.ImageUrl;
                        }
                    }
                    else
                    {
                        var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
                        if (existingVehicle != null &&
                            !string.IsNullOrEmpty(existingVehicle.ImageUrl) &&
                            existingVehicle.ImageUrl != "/images/vehicles/default-car.jpg")
                        {
                            DeleteOldImage(existingVehicle.ImageUrl);
                        }

                        vehicle.ImageUrl = await SaveImage(imageFile);
                    }

                    await _vehicleService.UpdateVehicleAsync(vehicle);
                    TempData["SuccessMessage"] = "Vehicle updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating vehicle");
                    TempData["ErrorMessage"] = "Error updating vehicle: " + ex.Message;
                }
            }
            else
            {
                _logger.LogWarning("ModelState is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }
                TempData["ErrorMessage"] = "Please correct the validation errors.";
            }

            await LoadBranchesViewBag();
            return View(vehicle);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null)
            {
                return NotFound();
            }

            bool hasActiveRentals = await _vehicleService.HasActiveRentalsAsync(vehicle.Id);
            bool hasAnyRentals = await _vehicleService.HasAnyRentalsAsync(vehicle.Id);

            ViewBag.ForceDelete = true;
            ViewBag.HasActiveRentals = hasActiveRentals;
            ViewBag.HasAnyRentals = hasAnyRentals;
            ViewBag.ForceDelete = false;

            if (hasActiveRentals)
            {
                ViewBag.WarningMessage = "This vehicle has active or future rentals.";
            }
            else if (hasAnyRentals)
            {
                ViewBag.WarningMessage = "This vehicle has past rental history.";
            }

            return View(vehicle);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool forceDelete = false)
        {
            try
            {
                bool result;

                if (forceDelete)
                {
                    _logger.LogWarning($"Force deleting vehicle ID: {id}");

                    
                    result = await _vehicleRepository.ForceDeleteVehicleAsync(id);

                    if (result)
                    {
                        TempData["SuccessMessage"] = "Vehicle and all associated records deleted successfully!";
                        _logger.LogInformation($"Vehicle {id} force deleted successfully");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Force delete operation failed!";
                        _logger.LogWarning($"Force delete failed for vehicle ID: {id}");
                    }
                }
                else
                {
                    _logger.LogInformation($"Attempting to delete vehicle ID: {id}");

                   
                    result = await _vehicleService.DeleteVehicleAsync(id);

                    if (result)
                    {
                        TempData["SuccessMessage"] = "Vehicle deleted successfully!";
                        _logger.LogInformation($"Vehicle {id} deleted successfully");
                    }
                    else
                    {
                        var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                        if (vehicle == null)
                        {
                            TempData["ErrorMessage"] = "Vehicle not found!";
                            _logger.LogWarning($"Vehicle {id} not found for deletion");
                        }
                        else if (!vehicle.IsAvailable)
                        {
                            bool hasActiveRentals = await _vehicleService.HasActiveRentalsAsync(id);
                            if (hasActiveRentals)
                            {
                                TempData["WarningMessage"] = "Vehicle has active rentals. It has been marked as unavailable.";
                                _logger.LogInformation($"Vehicle {id} marked as unavailable (has active rentals)");
                            }
                            else
                            {
                                TempData["WarningMessage"] = "Vehicle could not be deleted. It has been marked as unavailable.";
                                _logger.LogWarning($"Vehicle {id} could not be deleted, marked as unavailable");
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Vehicle could not be deleted.";
                            _logger.LogError($"Vehicle {id} could not be deleted for unknown reason");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting vehicle ID: {id}");
                TempData["ErrorMessage"] = $"An error occurred while deleting the vehicle: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ToggleAvailability(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (vehicle == null)
                {
                    TempData["ErrorMessage"] = "Vehicle not found!";
                    return RedirectToAction(nameof(Index));
                }

                if (!vehicle.IsAvailable)
                {
                    bool hasActiveRentals = await _vehicleService.HasActiveRentalsAsync(id);
                    if (hasActiveRentals)
                    {
                        TempData["ErrorMessage"] = "Cannot mark vehicle as available because it has active rentals.";
                        return RedirectToAction(nameof(Index));
                    }
                }

               
                vehicle.IsAvailable = !vehicle.IsAvailable;
                await _vehicleService.UpdateVehicleAsync(vehicle);

                string message = vehicle.IsAvailable
                    ? "Vehicle marked as available!"
                    : "Vehicle marked as unavailable!";

                TempData["SuccessMessage"] = message;
                _logger.LogInformation($"Vehicle {id} availability toggled to: {vehicle.IsAvailable}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling vehicle availability");
                TempData["ErrorMessage"] = "Error updating vehicle availability: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ForceDelete(int id)
        {
       
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only administrators can perform force delete operations.";
                return RedirectToAction(nameof(Index));
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
            {
                TempData["ErrorMessage"] = "Vehicle not found!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ForceDelete = true;
            ViewBag.WarningMessage = "⚠️ WARNING: Force delete will remove ALL associated records (rentals, stock, etc.). This action cannot be undone!";

            return View("Delete", vehicle);
        }

        private async Task LoadBranchesViewBag()
        {
            var branches = await _vehicleService.GetAllBranchesAsync();
            ViewBag.Branches = new SelectList(branches, "Id", "Name");
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "vehicles");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return $"/images/vehicles/{uniqueFileName}";
        }

        private void DeleteOldImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl == "/images/vehicles/default-car.jpg")
                return;

            try
            {
                var imagePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                    _logger.LogInformation($"Deleted old image: {imagePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting old image: {imageUrl}");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Available()
        {
            var availableVehicles = await _vehicleService.GetAvailableVehiclesAsync();
            return View(availableVehicles);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Search(string searchTerm, int? branchId)
        {
            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            var filteredVehicles = allVehicles.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                filteredVehicles = filteredVehicles.Where(v =>
                    (v.Brand != null && v.Brand.ToLower().Contains(searchTerm)) ||
                    (v.Model != null && v.Model.ToLower().Contains(searchTerm)) ||
                    (v.Color != null && v.Color.ToLower().Contains(searchTerm)) ||
                    (v.Description != null && v.Description.ToLower().Contains(searchTerm)));
            }

            if (branchId.HasValue && branchId > 0)
            {
                filteredVehicles = filteredVehicles.Where(v => v.BranchId == branchId.Value);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.BranchId = branchId;
            await LoadBranchesViewBag();

            return View(filteredVehicles.ToList());
        }
    }
}