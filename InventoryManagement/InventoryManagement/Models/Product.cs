
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Models
{
	// Represents a product in the catalog
	[Index(nameof(UPC), IsUnique = true)]
	public class Product
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "UPC is required")]
		[StringLength(50, MinimumLength = 8, ErrorMessage = "UPC must be between 8 and 50 characters")]
		[RegularExpression(@"^\d+$", ErrorMessage = "UPC must contain only numbers")]
		[Display(Name = "UPC Code")]
		public string UPC { get; set; } = string.Empty;

		[Required(ErrorMessage = "Product title is required")]
		[StringLength(500, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 500 characters")]
		[Display(Name = "Product Name")]
		public string Title { get; set; } = string.Empty;

		[StringLength(200, ErrorMessage = "Brand name cannot exceed 200 characters")]
		[Display(Name = "Brand")]
		public string? Brand { get; set; }

		[StringLength(200, ErrorMessage = "Model cannot exceed 200 characters")]
		[Display(Name = "Model Number")]
		public string? Model { get; set; }

		[StringLength(500, ErrorMessage = "Category cannot exceed 500 characters")]
		[Display(Name = "Category")]
		public string? Category { get; set; }

		[Url(ErrorMessage = "Please enter a valid URL")]
		[StringLength(1000, ErrorMessage = "Image URL cannot exceed 1000 characters")]
		[Display(Name = "Image URL")]
		public string? ImageUrl { get; set; }

		// Navigation property for Inventories
		public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
	}
}