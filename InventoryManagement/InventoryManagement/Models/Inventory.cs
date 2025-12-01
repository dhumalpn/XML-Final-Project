using InventoryManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Represents an inventory entry for a product

namespace InventoryManagement.Models
{
	public class Inventory
	{
		[Key]
		public int Id { get; set; }

		// Foreign Key
		[Required(ErrorMessage = "Product reference is required")]
		[Display(Name = "Product")]
		public int ProductId { get; set; }

		// Navigation Property
		[ForeignKey("ProductId")]
		public virtual required Product Product { get; set; }

		[Required(ErrorMessage = "Quantity is required")]
		[Range(0, 10000, ErrorMessage = "Quantity must be between 0 and 10,000")]
		[Display(Name = "Quantity")]
		public int Quantity { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Expiry Date")]
		[FutureDate(ErrorMessage = "Expiry date should be in the future")]
		public DateTime? ExpiryDate { get; set; }

		// Computed property for expiry status
		[Display(Name = "Expired")]
		public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Today;

		[StringLength(100, ErrorMessage = "Storage location cannot exceed 100 characters")]
		[Display(Name = "Storage Location")]
		[RegularExpression(@"^[a-zA-Z0-9\s\-,\.]+$",
		    ErrorMessage = "Storage location can only contain letters, numbers, spaces, hyphens, commas, and periods")]
		public string? StorageLocation { get; set; }

		[Required]
		[Display(Name = "Last Updated")]
		public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
	}

	// Custom validation attribute to ensure a date is in the future
	public class FutureDateAttribute : ValidationAttribute
	{
		public override bool IsValid(object? value)
		{
			if (value == null)
				return true; // Null is valid (for optional fields)

			if (value is DateTime dateValue)
			{
				return dateValue.Date >= DateTime.Today;
			}

			return false;
		}

		public override string FormatErrorMessage(string name)
		{
			return ErrorMessage ?? $"{name} must be today or a future date.";
		}
	}
}