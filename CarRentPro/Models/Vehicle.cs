using System.ComponentModel.DataAnnotations;

namespace CarRentPro.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Model is required")]
        [StringLength(100)]
        public string? Model { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        [StringLength(50)]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(2000, 2030, ErrorMessage = "Year must be between 2000 and 2030")]
        public int Year { get; set; } = 2020;

        [Required(ErrorMessage = "Color is required")]
        [StringLength(20)]
        public string? Color { get; set; }

        [Required(ErrorMessage = "Price per day is required")]
        [Range(0.01, 10000, ErrorMessage = "Price must be greater than 0")]
        public decimal PricePerDay { get; set; } = 50.00m;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000)]
        public string? Description { get; set; }

        
        [StringLength(255)]
        public string? ImageUrl { get; set; } = "/images/vehicles/default-car.jpg";

        public bool IsAvailable { get; set; } = true;

        
        [Required(ErrorMessage = "Branch is required")]
        public int BranchId { get; set; }
        public virtual Branch? Branch { get; set; }

        public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public virtual ICollection<VehicleStock> VehicleStocks { get; set; } = new List<VehicleStock>();
    }
}