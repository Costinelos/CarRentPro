using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CarRentPro.Models;
using CarRentPro.Services;
using Microsoft.EntityFrameworkCore;
using CarRentPro.Interfaces; // Adaugă această using directive

namespace CarRentPro.Controllers
{
    [Authorize]
    public class RentalController : Controller
    {
        private readonly IRentalService _rentalService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IBlacklistService _blacklistService; 

        public RentalController(IRentalService rentalService,
                              UserManager<ApplicationUser> userManager,
                              ApplicationDbContext context,
                              IBlacklistService blacklistService) 
        {
            _rentalService = rentalService;
            _userManager = userManager;
            _context = context;
            _blacklistService = blacklistService; 
        }

        public async Task<IActionResult> Rent(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.Branch)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            
            var user = await _userManager.GetUserAsync(User);
            if (user != null && user.IsBlacklisted)
            {
                TempData["ErrorMessage"] = "Your account is restricted. You cannot rent vehicles. Please contact support.";
                return RedirectToAction("Details", "Vehicle", new { id = vehicle.Id });
            }

            var isAvailable = await _rentalService.IsVehicleAvailableAsync(vehicle.Id);
            if (!isAvailable)
            {
                TempData["ErrorMessage"] = "This vehicle is currently not available for rental.";
                return RedirectToAction("Details", "Vehicle", new { id = vehicle.Id });
            }

            ViewBag.Vehicle = vehicle;
            ViewBag.MinReturnDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rent(int vehicleId, DateTime returnDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

           
            if (user.IsBlacklisted)
            {
                TempData["ErrorMessage"] = "Your account is restricted. You cannot rent vehicles. Please contact support.";
                return RedirectToAction("Details", "Vehicle", new { id = vehicleId });
            }

           
            var canRent = await _blacklistService.CheckIfUserCanRentAsync(user.Id);
            if (!canRent)
            {
                TempData["ErrorMessage"] = "Your account is restricted. You cannot rent vehicles. Please contact support.";
                return RedirectToAction("Details", "Vehicle", new { id = vehicleId });
            }

            if (returnDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Return date must be in the future.";
                return RedirectToAction("Rent", new { id = vehicleId });
            }

            var result = await _rentalService.CreateRentalAsync(user.Id, vehicleId, returnDate);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("MyRentals");
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Rent", new { id = vehicleId });
            }
        }

        public async Task<IActionResult> MyRentals()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            
            if (user.IsBlacklisted)
            {
                TempData["WarningMessage"] = "Your account is currently restricted. You cannot make new rentals.";
            }

            var rentals = await _rentalService.GetUserRentalsAsync(user.Id);
            return View(rentals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);

           
            if (user.IsBlacklisted)
            {
                TempData["ErrorMessage"] = "Your account is restricted. You cannot modify rentals.";
                return RedirectToAction("MyRentals");
            }

            var result = await _rentalService.CancelRentalAsync(id, user.Id);

            if (result)
            {
                TempData["SuccessMessage"] = "Rental cancelled successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error cancelling rental or rental not found.";
            }

            return RedirectToAction("MyRentals");
        }

        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Index()
        {
            var rentals = await _rentalService.GetAllRentalsAsync();
            return View(rentals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CompleteRental(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental != null)
            {
                rental.Status = "Completed";
                rental.Vehicle.IsAvailable = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rental marked as completed!";
            }
            else
            {
                TempData["ErrorMessage"] = "Rental not found!";
            }

            return RedirectToAction("Index");
        }

        
        [HttpGet]
        public async Task<IActionResult> CheckEligibility()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { canRent = false, message = "User not found" });
            }

            var canRent = await _blacklistService.CheckIfUserCanRentAsync(user.Id);
            return Json(new
            {
                canRent = canRent,
                message = canRent ? "You can rent vehicles" : "Your account is restricted"
            });
        }
    }
}