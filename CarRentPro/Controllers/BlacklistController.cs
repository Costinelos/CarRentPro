using CarRentPro.Interfaces;
using CarRentPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentPro.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class BlacklistController : Controller
    {
        private readonly IBlacklistService _blacklistService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BlacklistController(IBlacklistService blacklistService,
                                   UserManager<ApplicationUser> userManager,
                                   ApplicationDbContext context)
        {
            _blacklistService = blacklistService;
            _userManager = userManager;
            _context = context;
        }

        // GET: /Blacklist
        public async Task<IActionResult> Index()
        {
            var blacklist = await _blacklistService.GetBlacklistAsync();
            return View(blacklist);
        }

        // GET: /Blacklist/Add
        public async Task<IActionResult> Add()
        {
           
            var activeUsers = await _context.Users
                .Where(u => u.IsBlacklisted == false)
                .OrderBy(u => u.Email)
                .Select(u => new
                {
                    u.Id,
                    DisplayText = u.Email
                })
                .ToListAsync();

            ViewBag.ActiveUsers = activeUsers;
            return View();
        }

        // POST: /Blacklist/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string userId, string reason, DateTime? expirationDate)
        {
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "Please select a user.");
                return await Add();
            }

            if (string.IsNullOrEmpty(reason) || reason.Length < 10)
            {
                ModelState.AddModelError("reason", "Reason must be at least 10 characters.");
                return await Add();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return await Add();
            }

            
            var canRent = await _blacklistService.CheckIfUserCanRentAsync(user.Id);
            if (canRent)
            {
                await _blacklistService.AddToBlacklistAsync(
                    user.Id,
                    reason,
                    User.Identity?.Name ?? "System",
                    expirationDate
                );
                TempData["SuccessMessage"] = $"{user.Email} has been added to blacklist.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", $"{user.Email} is already blacklisted.");
                return await Add();
            }
        }

        // POST: /Blacklist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            await _blacklistService.RemoveFromBlacklistAsync(id, User.Identity?.Name ?? "System");
            TempData["SuccessMessage"] = "User has been removed from blacklist.";
            return RedirectToAction("Index");
        }

        // GET: /Blacklist/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var entry = await _blacklistService.GetBlacklistEntryAsync(id);
            if (entry == null)
            {
                return NotFound();
            }
            return View(entry);
            
        }
    }
}