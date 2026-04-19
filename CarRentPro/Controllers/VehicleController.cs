using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using CarRentPro.Models;
using CarRentPro.Services;
using CarRentPro.Repositories;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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
            if (id == null) return NotFound();

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null) return NotFound();

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

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        vehicle.ImageUrl = await SaveImageWithWatermark(imageFile);
                    }

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
            }

            await LoadBranchesViewBag();
            return View(vehicle);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null) return NotFound();

            await LoadBranchesViewBag();
            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehicle vehicle, IFormFile? imageFile)
        {
            if (id != vehicle.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVehicle = await _vehicleService.GetVehicleByIdAsync(id);
                    if (existingVehicle == null) return NotFound();

                    if (imageFile == null || imageFile.Length == 0)
                    {
                        vehicle.ImageUrl = existingVehicle.ImageUrl;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(existingVehicle.ImageUrl) &&
                            existingVehicle.ImageUrl != "/images/vehicles/default-car.jpg")
                        {
                            DeleteOldImage(existingVehicle.ImageUrl);
                        }

                        vehicle.ImageUrl = await SaveImageWithWatermark(imageFile);
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

        private async Task<string> SaveImageWithWatermark(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "vehicles");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                using (var stream = imageFile.OpenReadStream())
                using (var img = System.Drawing.Image.FromStream(stream))
                using (var g = System.Drawing.Graphics.FromImage(img))
                {
                    string watermarkPath = Path.Combine(_environment.WebRootPath, "images", "watermark.png");
                    if (System.IO.File.Exists(watermarkPath))
                    {
                        using (var watermark = System.Drawing.Image.FromFile(watermarkPath))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                            int watermarkWidth = img.Width / 4;
                            int watermarkHeight = (watermarkWidth * watermark.Height) / watermark.Width;

                            int xPosition = img.Width - watermarkWidth - 20; 
                            int yPosition = img.Height - watermarkHeight - 20;
                            g.DrawImage(watermark, new Rectangle(xPosition, yPosition, watermarkWidth, watermarkHeight));
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No watermark.png in wwwroot/images!");
                    }
                    img.Save(filePath, ImageFormat.Jpeg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Watermark failed, saving original: " + ex.Message);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id.Value);
            if (vehicle == null) return NotFound();

            bool hasActiveRentals = await _vehicleService.HasActiveRentalsAsync(vehicle.Id);
            bool hasAnyRentals = await _vehicleService.HasAnyRentalsAsync(vehicle.Id);

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
                    }
                }
                else
                {
                    _logger.LogInformation($"Attempting to delete vehicle ID: {id}");
                    result = await _vehicleService.DeleteVehicleAsync(id);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Vehicle deleted successfully!";
                    }
                    else
                    {
                        var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                        if (vehicle != null && !vehicle.IsAvailable)
                        {
                            TempData["WarningMessage"] = "Vehicle could not be deleted. It has been marked as unavailable.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting vehicle ID: {id}");
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ToggleAvailability(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                if (vehicle == null) return NotFound();

                if (!vehicle.IsAvailable)
                {
                    bool hasActiveRentals = await _vehicleService.HasActiveRentalsAsync(id);
                    if (hasActiveRentals)
                    {
                        TempData["ErrorMessage"] = "Cannot mark as available because it has active rentals.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                vehicle.IsAvailable = !vehicle.IsAvailable;
                await _vehicleService.UpdateVehicleAsync(vehicle);
                TempData["SuccessMessage"] = vehicle.IsAvailable ? "Vehicle available!" : "Vehicle unavailable!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling availability");
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ForceDelete(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only administrators can perform force delete.";
                return RedirectToAction(nameof(Index));
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();

            ViewBag.ForceDelete = true;
            ViewBag.WarningMessage = "⚠️ WARNING: This will remove ALL associated records. Action cannot be undone!";
            return View("Delete", vehicle);
        }

        private async Task LoadBranchesViewBag()
        {
            var branches = await _vehicleService.GetAllBranchesAsync();
            ViewBag.Branches = new SelectList(branches, "Id", "Name");
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
            var filtered = allVehicles.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                filtered = filtered.Where(v => 
                    (v.Brand != null && v.Brand.ToLower().Contains(searchTerm)) || 
                    (v.Model != null && v.Model.ToLower().Contains(searchTerm)) ||
                    (v.Color != null && v.Color.ToLower().Contains(searchTerm)));
            }

            if (branchId.HasValue && branchId > 0)
                filtered = filtered.Where(v => v.BranchId == branchId.Value);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.BranchId = branchId;
            await LoadBranchesViewBag();
            return View(filtered.ToList());
        }
    }
}