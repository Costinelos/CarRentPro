using System.ComponentModel.DataAnnotations;

namespace CarRentPro.Models
{
    public class PredefinedProfilePicture
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ImageName { get; set; }

        [Required]
        [StringLength(200)]
        public string ImagePath { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}