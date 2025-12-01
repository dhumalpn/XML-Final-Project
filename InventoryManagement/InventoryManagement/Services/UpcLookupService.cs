using InventoryManagement.Models;
using System.Text.Json;

namespace InventoryManagement.Services
{
    public class UpcLookupService : IUpcLookupService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UpcLookupService> _logger;

        public UpcLookupService(HttpClient httpClient, ILogger<UpcLookupService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Item?> LookupUpcAsync(string upc)
        {
            var url = $"https://api.upcitemdb.com/prod/trial/lookup?upc={upc}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("UPC API returned {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var parsed = ProductCodeResponse.FromJson(json);

                if (parsed == null || parsed.Total == 0 || parsed.Items == null || parsed.Items.Count == 0)
                {
                    return null;
                }

                return parsed.Items[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up UPC {Upc}", upc);
                return null;
            }
        }
    }
}
