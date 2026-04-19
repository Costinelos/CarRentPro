using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentPro.Models
{
    public class BlacklistEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedByAdminId { get; set; } 

        public bool IsActive { get; set; } = true;

        public DateTime? ExpirationDate { get; set; } 
    }
}