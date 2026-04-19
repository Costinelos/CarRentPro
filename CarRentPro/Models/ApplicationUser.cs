using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CarRentPro.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(500)]
        public string? Biography { get; set; }

        [StringLength(100)]
        public string? ProfilePicture { get; set; }

        public bool IsBlacklisted { get; set; } = false;

       
        public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();

        public virtual ICollection<BlacklistEntry> BlacklistEntries { get; set; } = new List<BlacklistEntry>();
    }
}