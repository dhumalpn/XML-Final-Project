using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "UPC is required")]
        [StringLength(50, ErrorMessage = "UPC cannot exceed 50 characters")]
        public string UPC { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Brand cannot exceed 100 characters")]
        public string? Brand { get; set; }

        [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
        public string? Model { get; set; }

        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string? Category { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number")]
        public int Quantity { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [StringLength(100, ErrorMessage = "Storage Location cannot exceed 100 characters")]
        public string? StorageLocation { get; set; }

        [StringLength(1, ErrorMessage = "NutriScore Grade must be a single character")]
        public string? NutriScoreGrade { get; set; }

        [StringLength(1, ErrorMessage = "EcoScore Grade must be a single character")]
        public string? EcoScoreGrade { get; set; }
    }
}
