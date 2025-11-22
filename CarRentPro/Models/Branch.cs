using System.ComponentModel.DataAnnotations;

namespace CarRentPro.Models
{
    public class Branch
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

   
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<VehicleStock> VehicleStocks { get; set; } = new List<VehicleStock>();
    }
}