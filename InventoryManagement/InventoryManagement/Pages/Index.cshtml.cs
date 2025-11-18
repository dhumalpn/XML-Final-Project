using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;


        private readonly InventoryManagementContext _db;

        private static readonly HttpClient client = new HttpClient();

        public IndexModel(ILogger<IndexModel> logger, InventoryManagementContext db)
        {
            _logger = logger;
            _db = db;
        }

        // Search input from UPC textbox
        [BindProperty]
        public string? UPC { get; set; }

        public Item? SearchedItem { get; set; }

        public List<Product> Products { get; set; } = new();

        // Which product is currently being edited (card in edit mode)
        [BindProperty]
        public int? EditProductId { get; set; }

        public async Task OnGetAsync()
        {
            await LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            Products = await _db.Product
                .OrderByDescending(p => p.Id)
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

            var url = $"https://api.upcitemdb.com/prod/trial/lookup?upc={UPC}";

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling UPCItemDB");
                ModelState.AddModelError(string.Empty, "Failed to reach UPCItemDB.");
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, $"UPCItemDB returned an error: {response.StatusCode}");
                return Page();
            }

            string json;
            try
            {
                json = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading UPCItemDB response");
                ModelState.AddModelError(string.Empty, "Failed to read response.");
                return Page();
            }

            ProductCodeResponse? parsed;
            try
            {
                parsed = ProductCodeResponse.FromJson(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing UPCItemDB response");
                ModelState.AddModelError(string.Empty, "Response format was unexpected.");
                return Page();
            }

            if (parsed == null || parsed.Total == 0 || parsed.Items == null || parsed.Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No product found for this UPC.");
                return Page();
            }

            SearchedItem = parsed.Items[0];
            return Page();
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

            var existing = await _db.Product.FirstOrDefaultAsync(p => p.UPC == upc);

            if (existing == null)
            {
                var product = new Product
                {
                    UPC = upc,
                    Title = title,
                    Brand = brand,
                    Model = model,
                    Category = category,
                    ImageUrl = imageUrl,
                    Quantity = quantity,
                    ExpiryDate = expiryDate,
                    StorageLocation = storageLocation
                };

                _db.Product.Add(product);
            }
            else
            {
                existing.Title = title;
                existing.Brand = brand;
                existing.Model = model;
                existing.Category = category;

                if (!string.IsNullOrWhiteSpace(imageUrl))
                    existing.ImageUrl = imageUrl;

                existing.Quantity += quantity;

                if (expiryDate.HasValue)
                    existing.ExpiryDate = expiryDate;

                if (!string.IsNullOrWhiteSpace(storageLocation))
                    existing.StorageLocation = storageLocation;
            }

            await _db.SaveChangesAsync();

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
            var product = await _db.Product.FindAsync(id);
            if (product == null)
                return NotFound();

            if (quantity < 0)
                quantity = 0;

            product.Quantity = quantity;
            product.ExpiryDate = expiryDate;
            product.StorageLocation = storageLocation;

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
            var product = await _db.Product.FindAsync(id);
            if (product == null)
                return NotFound();

            _db.Product.Remove(product);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Product removed from inventory.";
            return RedirectToPage();
        }
    }
}
