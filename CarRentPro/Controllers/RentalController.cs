using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CarRentPro.Models;
using CarRentPro.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRentPro.Controllers
{
    [Authorize]
    public class RentalController : Controller
    {
        private readonly IRentalService _rentalService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RentalController(IRentalService rentalService,
                              UserManager<ApplicationUser> userManager,
                              ApplicationDbContext context)
        {
            _rentalService = rentalService;
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int vehicleId, DateTime rentalDate, DateTime returnDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Identity");
            }

            var result = await _rentalService.CreateRentalAsync(user.Id, vehicleId, rentalDate, returnDate);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("MyRentals");
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Details", "Vehicle", new { id = vehicleId });
            }
        }

        public async Task<IActionResult> MyRentals()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Identity");
            }

            var rentals = await _rentalService.GetUserRentalsAsync(user.Id);
            return View(rentals);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (rental.UserId != user.Id && !User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                return Forbid();
            }

            
            rental.Status = "Cancelled";
            rental.Vehicle.IsAvailable = true;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rental cancelled successfully!";
            return RedirectToAction("MyRentals");
        }
    }
}