using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagement.Services;

namespace InventoryManagement.Pages
{
    public class InventoryModel : PageModel
    {
        private readonly OpenFoodFactsService _off;

        public List<InventoryItemVm> Items { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Place { get; set; } = string.Empty;

        // Constructor injection for the service
        public InventoryModel(OpenFoodFactsService off)
        {
            _off = off ?? throw new ArgumentNullException(nameof(off));
        }

        public void OnGet()
        {
            var allItems = new List<InventoryItemVm>
            {
                new InventoryItemVm { ProductName = "Chobani Greek Yogurt", Place = "Pantry", Quantity = 2, ExpiryDate = DateTime.Today.AddDays(5) },
                new InventoryItemVm { ProductName = "Milk", Place = "Pantry", Quantity = 0, ExpiryDate = DateTime.Today.AddDays(2) },
                new InventoryItemVm { ProductName = "Shampoo", Place = "Restroom", Quantity = 1 },
                new InventoryItemVm { ProductName = "Paper Towels", Place = "Restroom", Quantity = 1 },
                new InventoryItemVm { ProductName = "Frozen Berries", Place = "Freezer", Quantity = 3 }
            };

            if (!string.IsNullOrWhiteSpace(Place))
            {
                allItems = allItems
                    .Where(i => string.Equals(i.Place, Place, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(Search))
            {
                allItems = allItems
                    .Where(i => i.ProductName.Contains(Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            Items = allItems;
        }

        // UPC Lookup request/response classes
        public class UpcLookupRequest
        {
            public string Upc { get; set; } = "";
        }

        public class UpcLookupResponse
        {
            public bool Found { get; set; }
            public string? Title { get; set; }
            public string? Brand { get; set; }
            public string? Category { get; set; }
            public string? NutriScore { get; set; }
            public int? EcoScore { get; set; }
            public string? Description { get; set; }
            public string? Message { get; set; }
        }

        
        public async Task<IActionResult> OnPostLookupUpcAsync([FromBody] UpcLookupRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request?.Upc))
                return BadRequest(new UpcLookupResponse { Found = false, Message = "UPC required" });

            var dto = await _off.GetByUpcAsync(request.Upc, ct);

            if (!dto.Found)
            {
                return new JsonResult(new UpcLookupResponse
                {
                    Found = false,
                    Message = "Product not found in OpenFoodFacts."
                });
            }

            var resp = new UpcLookupResponse
            {
                Found = true,
                Title = dto.Title,
                Brand = dto.Brand,
                Category = dto.Category,
                NutriScore = dto.NutriScore,
                EcoScore = dto.EcoScore,
                Description = dto.Description
            };

            return new JsonResult(resp);
        }
    }

    // view model for displaying items
    public class InventoryItemVm
    {
        public string ProductName { get; set; } = "";
        public string Place { get; set; } = "";
        public int Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
