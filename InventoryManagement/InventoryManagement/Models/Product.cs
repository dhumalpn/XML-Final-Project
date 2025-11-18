using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Models
{
	[Index(nameof(UPC), IsUnique = true)] // Unique index on UPC
	public class Product
	{
		public int Id { get; set; }

		[Required]
		[StringLength(50)]
		public string UPC { get; set; } = string.Empty;

		[StringLength(500)]
		public string? Title { get; set; }

		[StringLength(200)]
		public string? Brand { get; set; }

		[StringLength(200)]
		public string? Model { get; set; }

		[StringLength(200)]
		public string? Category { get; set; }

		[StringLength(1000)]
		public string? ImageUrl { get; set; }

		public int Quantity { get; set; }
		public DateTime? ExpiryDate { get; set; }

		[StringLength(100)]
		public string? StorageLocation { get; set; }
	}
}