using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyBlazorApp.Models
{
    public class StockData
    {
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; }
        
        [JsonPropertyName("realtime_price")]
        public decimal RealtimePrice { get; set; }
        
        [JsonPropertyName("info")]
        public StockInfo Info { get; set; }
        
        [JsonPropertyName("about")]
        public About About { get; set; }
        
        [JsonPropertyName("graph_info")]
        public GraphInfo GraphInfo { get; set; }
    }

    public class StockInfo
    {
        [JsonPropertyName("previous_close")]
        public decimal PreviousClose { get; set; }
        
        [JsonPropertyName("day_range")]
        public DayRange DayRange { get; set; }
        
        [JsonPropertyName("year_range")]
        public YearRange YearRange { get; set; }
        
        [JsonPropertyName("market_cap")]
        public long MarketCap { get; set; }
        
        [JsonPropertyName("pe_ratio")]
        public decimal? PeRatio { get; set; }
        
        [JsonPropertyName("dividend_yield")]
        public decimal? DividendYield { get; set; }
        
        [JsonPropertyName("primary_exchange")]
        public string PrimaryExchange { get; set; }
        
        [JsonPropertyName("average_volume")]
        public long AverageVolume { get; set; }
    }

    public class DayRange
    {
        [JsonPropertyName("low")]
        public decimal Low { get; set; }
        
        [JsonPropertyName("high")]
        public decimal High { get; set; }
    }

    public class YearRange
    {
        [JsonPropertyName("low")]
        public decimal? Low { get; set; }
        
        [JsonPropertyName("high")]
        public decimal? High { get; set; }
    }

    public class About
    {
        [JsonPropertyName("About")]
        public string Description { get; set; }
    }

    public class GraphInfo
    {
        [JsonPropertyName("historical_prices")]
        public List<PriceData> HistoricalPrices { get; set; }
        
        [JsonPropertyName("hourly_prices")]
        public List<PriceData> HourlyPrices { get; set; }
    }

    public class PriceData
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }
        
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }
}
