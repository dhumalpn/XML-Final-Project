using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InventoryManagement.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;
		private readonly InventoryManagementContext _db;
		private readonly IHttpClientFactory _httpClientFactory;

		public IndexModel(
		    ILogger<IndexModel> logger,
		    InventoryManagementContext db,
		    IHttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_db = db;
			_httpClientFactory = httpClientFactory;
		}

		// Search input from UPC textbox
		[BindProperty]
		public string? UPC { get; set; }

		public Item? SearchedItem { get; set; }

		// Changed from List<Product> to List<Inventory>
		public List<Inventory> Products { get; set; } = new();

		// Which inventory item is currently being edited
		[BindProperty]
		public int? EditProductId { get; set; }

		public async Task OnGetAsync()
		{
			// Fetch Inventory items with their related Product details
			Products = await _db.Inventory
			    .Include(i => i.Product)
			    .OrderByDescending(i => i.LastUpdated)
			    .ToListAsync();
		}

		private async Task LoadProductsAsync()
		{
			Products = await _db.Inventory
			    .Include(i => i.Product)
			    .OrderByDescending(i => i.LastUpdated)
			    .ToListAsync();
		}

		public async Task<IActionResult> OnPostSearchAsync()
		{
			await LoadProductsAsync();

			if (string.IsNullOrWhiteSpace(UPC))
			{
				ModelState.AddModelError(nameof(UPC), "Please enter a UPC.");
				return Page();
			}

			var client = _httpClientFactory.CreateClient("UPCItemDB");
			var url = $"lookup?upc={Uri.EscapeDataString(UPC)}";

			_logger.LogInformation("Looking up UPC {UPC} from UPCItemDB", UPC);

			try
			{
				using var response = await client.GetAsync(url);

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogWarning(
					    "UPCItemDB lookup failed for UPC {UPC}. Status: {StatusCode}, Reason: {Reason}",
					    UPC,
					    response.StatusCode,
					    response.ReasonPhrase
					);

					var userMessage = response.StatusCode switch
					{
						System.Net.HttpStatusCode.NotFound => "Product not found. Please verify the UPC code.",
						System.Net.HttpStatusCode.TooManyRequests => "API rate limit exceeded. Please try again later.",
						System.Net.HttpStatusCode.ServiceUnavailable => "Product database is temporarily unavailable.",
						_ => "Unable to lookup product at this time. Please try again."
					};

					ModelState.AddModelError(string.Empty, userMessage);
					return Page();
				}

				var json = await response.Content.ReadAsStringAsync();

				ProductCodeResponse? parsed;
				try
				{
					parsed = ProductCodeResponse.FromJson(json);
				}
				catch (JsonException ex)
				{
					_logger.LogError(ex,
					    "Failed to parse UPCItemDB response for UPC {UPC}. Response length: {Length}",
					    UPC,
					    json?.Length ?? 0
					);

					if (json != null)
					{
						var previewLength = Math.Min(500, json.Length);
						_logger.LogDebug("Response preview: {Preview}", json[..previewLength]);
					}

					ModelState.AddModelError(string.Empty,
					    "Received unexpected data format from product database. Our team has been notified.");
					return Page();
				}

				if (parsed == null || parsed.Total == 0 || parsed.Items == null || !parsed.Items.Any())
				{
					_logger.LogInformation("No product found for UPC {UPC}", UPC);
					ModelState.AddModelError(string.Empty,
					    "No product found for this UPC. Double-check the code or try adding manually.");
					return Page();
				}

				SearchedItem = parsed.Items[0];
				_logger.LogInformation(
				    "Successfully found product {Title} for UPC {UPC}",
				    SearchedItem.Title,
				    UPC
				);
				return Page();
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Error calling UPC {UPC} from UPCItemDB", UPC);
				ModelState.AddModelError(string.Empty,
				    "Network error. Failed to reach UPCItemDB.");
				return Page();
			}
			catch (TaskCanceledException ex)
			{
				_logger.LogError(ex, "Timeout while looking up UPC {UPC}", UPC);
				ModelState.AddModelError(string.Empty,
				    "Request timed out. The product database may be slow. Please try again.");
				return Page();
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("UPC lookup cancelled for {UPC}", UPC);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error looking up UPC {UPC}", UPC);
				ModelState.AddModelError(string.Empty,
				    "An unexpected error occurred. Please try again or contact support if the problem persists.");
				return Page();
			}
		}

		public async Task<IActionResult> OnPostAddAsync(
		    string upc,
		    string title,
		    string? brand,
		    string? model,
		    string? category,
		    string? imageUrl,
		    int quantity,
		    DateTime? expiryDate,
		    string? storageLocation)
		{
			await LoadProductsAsync();

			if (string.IsNullOrWhiteSpace(upc) || string.IsNullOrWhiteSpace(title))
			{
				ModelState.AddModelError(string.Empty, "UPC and Title are required.");
				return Page();
			}

			if (quantity < 0)
				quantity = 0;

			// Find or create Product
			var existingProduct = await _db.Product.FirstOrDefaultAsync(p => p.UPC == upc);

			if (existingProduct == null)
			{
				try
				{
					// Create new Product
					var product = new Product
					{
						UPC = upc,
						Title = title,
						Brand = brand,
						Model = model,
						Category = category,
						ImageUrl = imageUrl
					};
					_db.Product.Add(product);
					await _db.SaveChangesAsync();

					// Create Inventory entry for this product
					var inventory = new Inventory
					{
						ProductId = product.Id,
						Product = product,
						Quantity = quantity,
						ExpiryDate = expiryDate,
						StorageLocation = storageLocation,
						LastUpdated = DateTime.UtcNow
					};
					_db.Inventory.Add(inventory);
					await _db.SaveChangesAsync();
				}
				catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Product_UPC") == true)
				{
					ModelState.AddModelError(string.Empty, "This UPC already exists in inventory.");
					return Page();
				}
			}
			else
			{
				// Update existing product info
				existingProduct.Title = title;
				existingProduct.Brand = brand;
				existingProduct.Model = model;
				existingProduct.Category = category;

				if (!string.IsNullOrWhiteSpace(imageUrl))
					existingProduct.ImageUrl = imageUrl;

				// Check if an inventory entry already exists for this product
				var existingInventory = await _db.Inventory
				    .FirstOrDefaultAsync(i => i.ProductId == existingProduct.Id);

				if (existingInventory != null)
				{
					// Update existing inventory
					existingInventory.Quantity += quantity;

					if (expiryDate.HasValue)
						existingInventory.ExpiryDate = expiryDate;

					if (!string.IsNullOrWhiteSpace(storageLocation))
						existingInventory.StorageLocation = storageLocation;

					existingInventory.LastUpdated = DateTime.UtcNow;
				}
				else
				{
					// Create new inventory entry for existing product
					var inventory = new Inventory
					{
						ProductId = existingProduct.Id,
						Product = existingProduct,
						Quantity = quantity,
						ExpiryDate = expiryDate,
						StorageLocation = storageLocation,
						LastUpdated = DateTime.UtcNow
					};
					_db.Inventory.Add(inventory);
				}

				await _db.SaveChangesAsync();
			}

			TempData["Message"] = "Product added to your inventory.";
			return RedirectToPage();
		}

		public async Task<IActionResult> OnPostStartEditAsync(int id)
		{
			EditProductId = id;
			await LoadProductsAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostUpdateAsync(
		    int id,
		    int quantity,
		    DateTime? expiryDate,
		    string? storageLocation)
		{
			var inventory = await _db.Inventory.FindAsync(id);
			if (inventory == null)
				return NotFound();

			if (quantity < 0)
				quantity = 0;

			inventory.Quantity = quantity;
			inventory.ExpiryDate = expiryDate;
			inventory.StorageLocation = storageLocation;
			inventory.LastUpdated = DateTime.UtcNow;

			await _db.SaveChangesAsync();

			TempData["Message"] = "Product updated.";
			return RedirectToPage();
		}

		public async Task<IActionResult> OnPostCancelEditAsync()
		{
			EditProductId = null;
			await LoadProductsAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(int id)
		{
			var inventory = await _db.Inventory.FindAsync(id);
			if (inventory == null)
				return NotFound();

			_db.Inventory.Remove(inventory);
			await _db.SaveChangesAsync();

			TempData["Message"] = "Product removed from inventory.";
			return RedirectToPage();
		}
	}
}