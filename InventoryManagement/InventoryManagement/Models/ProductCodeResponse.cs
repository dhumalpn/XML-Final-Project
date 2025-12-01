// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:	using InventoryManagement.Models;	var productCodeResponse = ProductCodeResponse.FromJson(jsonString);

namespace InventoryManagement.Models
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Globalization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	// Response from UPC lookup API
	public partial class ProductCodeResponse
	{
		[JsonProperty("code")]
		[Required]
		public string Code { get; set; } = string.Empty;

		[JsonProperty("total")]
		[Range(0, int.MaxValue)]
		public long Total { get; set; }

		[JsonProperty("offset")]
		[Range(0, int.MaxValue)]
		public long Offset { get; set; }

		[JsonProperty("items")]
		public List<Item> Items { get; set; } = new List<Item>();
	}

	// Product item from API response
	public partial class Item
	{
		[JsonProperty("ean")]
		[StringLength(50)]
		public string? Ean { get; set; }

		[JsonProperty("title")]
		[Required(ErrorMessage = "Product title is required")]
		[StringLength(500, MinimumLength = 1)]
		public string Title { get; set; } = string.Empty;

		[JsonProperty("description")]
		[StringLength(2000)]
		public string? Description { get; set; }

		[JsonProperty("upc")]
		[Required]
		[RegularExpression(@"^\d+$", ErrorMessage = "UPC must contain only numbers")]
		[StringLength(50, MinimumLength = 8)]
		public string Upc { get; set; } = string.Empty;

		[JsonProperty("brand")]
		[StringLength(200)]
		public string? Brand { get; set; }

		[JsonProperty("model")]
		[StringLength(200)]
		public string? Model { get; set; }

		[JsonProperty("color")]
		[StringLength(100)]
		public string? Color { get; set; }

		[JsonProperty("size")]
		[StringLength(100)]
		public string? Size { get; set; }

		[JsonProperty("dimension")]
		[StringLength(200)]
		public string? Dimension { get; set; }

		[JsonProperty("weight")]
		[StringLength(100)]
		public string? Weight { get; set; }

		[JsonProperty("category")]
		[StringLength(500)]
		public string? Category { get; set; }

		[JsonProperty("lowest_recorded_price")]
		[Range(0, double.MaxValue)]
		public long LowestRecordedPrice { get; set; }

		[JsonProperty("highest_recorded_price")]
		[Range(0, double.MaxValue)]
		public long HighestRecordedPrice { get; set; }

		[JsonProperty("images")]
		public List<Uri> Images { get; set; } = new List<Uri>();

		[JsonProperty("offers")]
		public List<Offer> Offers { get; set; } = new List<Offer>();

		[JsonProperty("elid")]
		[StringLength(100)]
		public string? Elid { get; set; }
	}

	// Product offer from merchant
	public partial class Offer
	{
		[JsonProperty("merchant")]
		[StringLength(200)]
		public string? Merchant { get; set; }

		[JsonProperty("domain")]
		[StringLength(500)]
		[Url]
		public string? Domain { get; set; }

		[JsonProperty("title")]
		[StringLength(500)]
		public string? Title { get; set; }

		[JsonProperty("currency")]
		public Currency Currency { get; set; }

		[JsonProperty("list_price")]
		public ListPrice ListPrice { get; set; }

		[JsonProperty("price")]
		[Range(0, double.MaxValue)]
		public double Price { get; set; }

		[JsonProperty("shipping")]
		[StringLength(100)]
		public string? Shipping { get; set; }

		[JsonProperty("condition")]
		public Condition Condition { get; set; }

		[JsonProperty("availability")]
		[StringLength(100)]
		public string? Availability { get; set; }

		[JsonProperty("link")]
		[Url]
		public Uri? Link { get; set; }

		[JsonProperty("updated_t")]
		public long UpdatedT { get; set; }
	}

	public enum Condition { New, Used, Refurbished, Unknown }

	public enum Currency { USD, CAD, EUR, GBP, Empty }

	public partial struct ListPrice
	{
		public double? Double;
		public string? String;

		public static implicit operator ListPrice(double Double) => new ListPrice { Double = Double };
		public static implicit operator ListPrice(string String) => new ListPrice { String = String };
	}

	public partial class ProductCodeResponse
	{
		public static ProductCodeResponse FromJson(string json) =>
		    JsonConvert.DeserializeObject<ProductCodeResponse>(json, InventoryManagement.Models.Converter.Settings)
		    ?? throw new JsonException("Failed to deserialize ProductCodeResponse");
	}

	public static class Serialize
	{
		public static string ToJson(this ProductCodeResponse self) =>
		    JsonConvert.SerializeObject(self, InventoryManagement.Models.Converter.Settings);
	}

	internal static class Converter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			NullValueHandling = NullValueHandling.Ignore,
			Converters =
		  {
			 ConditionConverter.Singleton,
			 CurrencyConverter.Singleton,
			 ListPriceConverter.Singleton,
			 new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
		  },
		};
	}

	internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

    internal class ConditionConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Condition) || t == typeof(Condition?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "New")
            {
                return Condition.New;
            }
            throw new Exception("Cannot unmarshal type Condition");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Condition)untypedValue;
            if (value == Condition.New)
            {
                serializer.Serialize(writer, "New");
                return;
            }
            throw new Exception("Cannot marshal type Condition");
        }

        public static readonly ConditionConverter Singleton = new ConditionConverter();
    }

    internal class CurrencyConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Currency) || t == typeof(Currency?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "":
                    return Currency.Empty;
                case "CAD":
                    return Currency.CAD;
                case "GBP":
                    return Currency.GBP;
            }
            throw new Exception("Cannot unmarshal type Currency");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Currency)untypedValue;
            switch (value)
            {
                case Currency.Empty:
                    serializer.Serialize(writer, "");
                    return;
                case Currency.CAD:
                    serializer.Serialize(writer, "CAD");
                    return;
                case Currency.GBP:
                    serializer.Serialize(writer, "GBP");
                    return;
            }
            throw new Exception("Cannot marshal type Currency");
        }

        public static readonly CurrencyConverter Singleton = new CurrencyConverter();
    }

    internal class ListPriceConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(ListPrice) || t == typeof(ListPrice?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    var doubleValue = serializer.Deserialize<double>(reader);
                    return new ListPrice { Double = doubleValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new ListPrice { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type ListPrice");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (ListPrice)untypedValue;
            if (value.Double != null)
            {
                serializer.Serialize(writer, value.Double.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type ListPrice");
        }

        public static readonly ListPriceConverter Singleton = new ListPriceConverter();
    }
}
