using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentPro.Models;
using CarRentPro.Services;
using System.Diagnostics;

namespace CarRentPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IGroqService _groqService;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IGroqService groqService)
        {
            _context = context;
            _logger = logger;
            _groqService = groqService;
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


        [HttpPost]
        public async Task<IActionResult> GetAiRecommendation([FromBody] string userMessage)
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.IsAvailable)
                .Select(v => $"ID: {v.Id} | {v.Brand} {v.Model} ({v.Year}, {v.PricePerDay}$/day, {v.Color})")
                .ToListAsync();

            var contextString = string.Join("\n", vehicles);
            var response = await _groqService.GetRecommendationAsync(userMessage, contextString);
            return Json(new { reply = response });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}