using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Services
{
    // DTO returned to your page model
    public class OffProductDto
    {
        public bool Found { get; set; }
        public string? Title { get; set; }
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public string? NutriScore { get; set; }
        public int? EcoScore { get; set; }
        public string? Description { get; set; }
    }

    public class OpenFoodFactsService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<OpenFoodFactsService> _log;

        // Cache results for 6 hours to reduce API calls
        private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
        };

        public OpenFoodFactsService(HttpClient http, IMemoryCache cache, ILogger<OpenFoodFactsService> log)
        {
            _http = http;
            _cache = cache;
            _log = log;
        }

        public async Task<OffProductDto> GetByUpcAsync(string upc, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(upc))
                return new OffProductDto { Found = false };

            upc = upc.Trim();

            // Try cached result first
            if (_cache.TryGetValue(upc, out OffProductDto? cached))
                return cached!;

            try
            {
                var url = $"https://world.openfoodfacts.org/api/v0/product/{Uri.EscapeDataString(upc)}.json";
                using var resp = await _http.GetAsync(url, ct);

                if (!resp.IsSuccessStatusCode)
                {
                    _log.LogWarning("OpenFoodFacts returned {Status} for UPC {Upc}", resp.StatusCode, upc);
                    var fail = new OffProductDto { Found = false };
                    _cache.Set(upc, fail, _cacheOptions);
                    return fail;
                }

                using var stream = await resp.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                var root = doc.RootElement;

                // OFF uses status = 1 for "found"
                if (root.TryGetProperty("status", out var statusEl) && statusEl.GetInt32() == 1 &&
                    root.TryGetProperty("product", out var productEl))
                {
                    var dto = new OffProductDto { Found = true };

                    // Product title
                    if (productEl.TryGetProperty("product_name", out var pn) && pn.ValueKind == JsonValueKind.String)
                        dto.Title = pn.GetString();
                    else if (productEl.TryGetProperty("generic_name", out var gn) && gn.ValueKind == JsonValueKind.String)
                        dto.Title = gn.GetString();

                    // Brand
                    if (productEl.TryGetProperty("brands", out var br) && br.ValueKind == JsonValueKind.String)
                        dto.Brand = br.GetString();

                    // Category (take first if multiple)
                    if (productEl.TryGetProperty("categories", out var cat) && cat.ValueKind == JsonValueKind.String)
                    {
                        var catStr = cat.GetString();
                        if (!string.IsNullOrWhiteSpace(catStr))
                            dto.Category = catStr.Split(',').Select(s => s.Trim()).FirstOrDefault();
                    }

                    // NutriScore (letter)
                    if (productEl.TryGetProperty("nutriscore_grade", out var ns) && ns.ValueKind == JsonValueKind.String)
                        dto.NutriScore = ns.GetString()?.ToUpperInvariant();

                    // EcoScore (numeric)
                    if (productEl.TryGetProperty("ecoscore_score", out var eco) && eco.ValueKind == JsonValueKind.Number)
                        dto.EcoScore = eco.GetInt32();

                    // Description / Ingredients
                    if (productEl.TryGetProperty("ingredients_text", out var ing) && ing.ValueKind == JsonValueKind.String)
                        dto.Description = ing.GetString();

                    _cache.Set(upc, dto, _cacheOptions);
                    return dto;
                }

                // Product not found
                var notFound = new OffProductDto { Found = false };
                _cache.Set(upc, notFound, _cacheOptions);
                return notFound;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error looking up UPC {Upc}", upc);
                var errorDto = new OffProductDto { Found = false };
                _cache.Set(upc, errorDto, _cacheOptions);
                return errorDto;
            }
        }
    }
}
