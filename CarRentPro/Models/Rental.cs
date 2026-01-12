using System.ComponentModel.DataAnnotations;

namespace CarRentPro.Models
{
    public class Rental
    {
        public int Id { get; set; }

        public DateTime RentalDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalPrice { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Active, Completed, Cancelled

        
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int? VehicleId { get; set; }
        public virtual Vehicle? Vehicle { get; set; }

        
        public bool IsActive()
        {
            return Status == "Active" && DateTime.Now >= RentalDate && DateTime.Now <= ReturnDate;
        }
    }
}