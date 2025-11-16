using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryManagement.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private static readonly HttpClient client = new HttpClient();

        [BindProperty]
        public string? UPC { get; set; }

        public Item? SearchedItem { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
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
            catch
            {
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
            catch
            {
                ModelState.AddModelError(string.Empty, "Failed to read response.");
                return Page();
            }

            ProductCodeResponse? parsed;
            try
            {
                parsed = ProductCodeResponse.FromJson(json);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Response format was unexpected.");
                return Page();
            }

            if (parsed == null || parsed.Total == 0 || parsed.Items == null || parsed.Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No product found for this UPC.");
                return Page();
            }

            // Return the first product
            SearchedItem = parsed.Items[0];

            return Page();
        }
    }
}
