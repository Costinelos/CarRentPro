using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentPro.Models;

namespace CarRentPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Include(v => v.Branch)
                    .Where(v => v.IsAvailable)
                    .ToListAsync();
                return View(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vehicles for homepage");
                return View(new List<Vehicle>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}