using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace InventoryManagement.Pages
{
    public class InventoryModel : PageModel
    {
        public List<InventoryItemVm> Items { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Place { get; set; }

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
    }
    public class InventoryItemVm
    {
        public string ProductName { get; set; } = "";
        public string Place { get; set; } = "";
        public int Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}


