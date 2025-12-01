using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string UPC { get; set; } = string.Empty;
        public string? Title { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? StorageLocation { get; set; }
        public string? NutriScoreGrade { get; set; }
        public string? EcoScoreGrade { get; set; }
    }
}
