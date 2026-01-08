using CarRentPro.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CarRentPro.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _env;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }

        public string Username { get; set; }
        [TempData] public string StatusMessage { get; set; }
        [BindProperty] public InputModel Input { get; set; }
        public List<string> AvailableProfilePictures { get; set; } = new();

        public class InputModel
        {
            
            [Display(Name = "Telefon")]
            public string? PhoneNumber { get; set; } 

            [MaxLength(500)]
            [Display(Name = "Biografie")]
            public string? Biography { get; set; }

            [Display(Name = "Poza de profil")]
            public string? ProfilePicture { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;
            Input = new InputModel
            {
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                Biography = user.Biography,
                ProfilePicture = user.ProfilePicture
            };
            AvailableProfilePictures = GetAvailableProfilePictures();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                AvailableProfilePictures = GetAvailableProfilePictures();
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            }

            user.Biography = Input.Biography;
            user.ProfilePicture = Input.ProfilePicture;
            await _userManager.UpdateAsync(user);

            StatusMessage = "Profil actualizat!";
            return RedirectToPage();
        }

        private List<string> GetAvailableProfilePictures()
        {
            var path = Path.Combine(_env.WebRootPath, "images", "profile-pictures");
            if (!Directory.Exists(path)) return new List<string> { "default.png" };

            return Directory.GetFiles(path, "*.*")
                .Where(f => f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".jpeg"))
                .Select(Path.GetFileName)
                .ToList();
        }
    }
}