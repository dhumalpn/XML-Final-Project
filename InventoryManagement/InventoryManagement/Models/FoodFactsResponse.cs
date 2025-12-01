namespace InventoryManagement.Models
{
    using Newtonsoft.Json;

    public partial class FoodFactsResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("product")]
        public ProductDetails Product { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("status_verbose")]
        public string StatusVerbose { get; set; }
    }

    public partial class ProductDetails
    {
        [JsonProperty("nutriscore_grade")]
        public string NutriscoreGrade { get; set; }
        [JsonProperty("ecoscore_grade")]
        public string EcoscoreGrade { get; set; }
    }

    public partial class FoodFactsResponse
    {
        public static FoodFactsResponse FromJson(string json) => JsonConvert.DeserializeObject<FoodFactsResponse>(json, InventoryManagement.Models.Converter.Settings);
    }
}