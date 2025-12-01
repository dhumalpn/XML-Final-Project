using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "UPC is required")]
        [StringLength(20, ErrorMessage = "UPC cannot exceed 20 characters")]
        public string UPC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string? Title { get; set; } = string.Empty;

        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? StorageLocation { get; set; }
    }
}
