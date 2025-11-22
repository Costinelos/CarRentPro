using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CarRentPro.Models;
using CarRentPro.Services;

namespace CarRentPro.Controllers
{
    public class VehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehicleController> _logger;
        private readonly IWebHostEnvironment _environment;

        public VehicleController(IVehicleService vehicleService, ILogger<VehicleController> logger, IWebHostEnvironment environment)
        {
            _vehicleService = vehicleService;
            _logger = logger;
            _environment = environment;
        }

        // GET: Vehicle/Index 
        public async Task<IActionResult> Index()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return View(vehicles);
        }

        // GET: Vehicle/Details/5
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

        // GET: Vehicle/Create
        public async Task<IActionResult> Create()
        {
            await LoadBranchesViewBag();
            return View();
        }

        // POST: Vehicle/Create
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

        // GET: Vehicle/Edit/5
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

        // POST: Vehicle/Edit/5
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
                    
                    if (imageFile != null && imageFile.Length > 0)
                    {
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

            await LoadBranchesViewBag();
            return View(vehicle);
        }

        // GET: Vehicle/Delete/5
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

            return View(vehicle);
        }

        // POST: Vehicle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _vehicleService.DeleteVehicleAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Vehicle deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Vehicle not found!";
            }

            return RedirectToAction(nameof(Index));
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
    }
}