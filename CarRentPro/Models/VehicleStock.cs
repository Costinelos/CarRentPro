namespace CarRentPro.Models
{
    public class VehicleStock
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }

       
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; }

        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; }
    }
}