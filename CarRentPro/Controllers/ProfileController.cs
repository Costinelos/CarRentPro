using CarRentPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentPro.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Preia pozele predefinite din baza de date
            var pictures = await _context.PredefinedProfilePictures
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.ImageName)
                .ToListAsync();

            // Trimite datele la View prin ViewBag
            ViewBag.User = user;
            ViewBag.Pictures = pictures;
            ViewBag.Categories = pictures.Select(p => p.Category).Distinct().ToList();

            return View();
        }

        // POST: /Profile/UpdateBio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBio(string biography)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validare biografie
            if (biography != null && biography.Length > 500)
            {
                TempData["ErrorMessage"] = "Biography cannot exceed 500 characters.";
                return RedirectToAction("Index");
            }

            // Actualizează biografia
            user.Biography = biography;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Biography updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error updating biography.";
            }

            return RedirectToAction("Index");
        }

        // POST: /Profile/UpdatePicture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePicture(string picturePath)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(picturePath))
            {
                TempData["ErrorMessage"] = "Please select a profile picture.";
                return RedirectToAction("Index");
            }

            // Verifică dacă poza este validă (există în baza de date)
            var isValidPicture = await _context.PredefinedProfilePictures
                .AnyAsync(p => p.ImagePath == picturePath && p.IsActive);

            if (!isValidPicture)
            {
                TempData["ErrorMessage"] = "Invalid profile picture selected.";
                return RedirectToAction("Index");
            }

            // Actualizează poza de profil
            user.ProfilePicture = picturePath;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile picture updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error updating profile picture.";
            }

            return RedirectToAction("Index");
        }
    }
}