using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Globalization;
using MyBlazorApp.Models;
using System.Text.Json.Serialization;
using System.Text.Json;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    //API call to get stock information. Data stored as type StockData which is a class that can be found in Models folder
    public async Task<StockData> FetchStockData(string ticker)
    {
        if (string.IsNullOrEmpty(ticker))
        {
            throw new ArgumentException("Ticker can't be empty", nameof(ticker));
        }

        var response = await _httpClient.PostAsJsonAsync("/api/search", new { ticker = ticker.ToUpper() });
        if (response.IsSuccessStatusCode)
        {
            var stockData = await response.Content.ReadFromJsonAsync<StockData>();
            return stockData;
        }
        else
        {
            // Handle error
            throw new Exception($"Failed to fetch stock data for {ticker.ToUpper()}");
        }
    }

    
    public async Task<HttpResponseMessage> FetchDashboardData(List<string> portfolios)
    {
        var data = new
        {
            action = "find_dashboard_data",
            query1 = new { ticker = new Dictionary<string, List<string>> { { "$in", portfolios } } },
            query2 = new { },
            query3 = new { }
        };

        return await _httpClient.PostAsJsonAsync("/api/history_persist", data);
    }

    public async Task<HttpResponseMessage> FetchTickers(string username = "")
    {
        var data = new
        {
            action = "get_tickers",
            username
        };

        return await _httpClient.PostAsJsonAsync("/api/portfolio", data);
    }

    public async Task<HttpResponseMessage> AddToPortfolio(string ticker, string username = "")
    {
        var data = new
        {
            action = "upsert",
            query1 = new { username },
            query2 = new { ticker },
            update_values = new { date_added = DateTime.UtcNow.ToString("MM/dd/yyyy") }
        };

        var response = await _httpClient.PostAsJsonAsync("/api/persist", data);
        if (response.IsSuccessStatusCode)
        {
            // Success notification logic here
            Console.WriteLine($"Successfully Added {ticker} to your Portfolio");
        }
        return response;
    }

    public async Task<HttpResponseMessage> RemoveFromPortfolio(string ticker, string username = "")
    {
        var data = new
        {
            action = "delete",
            query1 = new { username },
            query2 = new { ticker }
        };

        var response = await _httpClient.PostAsJsonAsync("/api/persist", data);
        if (response.IsSuccessStatusCode)
        {
            // Success notification logic here
            Console.WriteLine($"Successfully Removed {ticker} from your Portfolio");
        }
        return response;
    }

    public async Task<HttpResponseMessage> FetchHistoryDataAsHttpResponse(List<string> tickers, (DateTime startDate, DateTime endDate) dateRange)
    {
        var dateQuery = new Dictionary<string, string>
        {
            ["$lte"] = dateRange.endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["$gte"] = dateRange.startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };

        var tickerQuery = new Dictionary<string, List<string>>
        {
            ["$in"] = tickers
        };

        var data = new
        {
            action = "findall",
            query1 = new { ticker = tickerQuery },
            query2 = new { date = dateQuery },
            query3 = new { }
        };

        var response = await _httpClient.PostAsJsonAsync("/api/history_persist", data);
        response.EnsureSuccessStatusCode();
        return response;
    }

    public async Task<List<HistoryData>> FetchHistoryData(List<string> tickers, (DateTime startDate, DateTime endDate) dateRange)
    {
        var response = await FetchHistoryDataAsHttpResponse(tickers, dateRange);
        return await response.Content.ReadFromJsonAsync<List<HistoryData>>();
    }

    public async Task<HttpResponseMessage> UpdateTicker(string action, string collection, object query, string trimmedTicker, object data)
    {
        var formattedDate = DateTime.Now.ToString("MM/dd/yyyy");
        var ticker_data = new { symbol = trimmedTicker, date = formattedDate };

        // Add ticker logic here
        // Ensure `data` contains a `ticker` array, add `ticker_data` to it

        var requestData = new
        {
            action,
            collection,
            query,
            update_values = data
        };

        var response = await _httpClient.PostAsJsonAsync("/api/persist", requestData);
        return response;
    }

    public async Task<HttpResponseMessage> DeleteTicker(string action, string collection, object query, string trimmedTicker, object data)
    {
        // Remove ticker logic here
        // Ensure `data` contains a `ticker` array, remove `trimmedTicker` from it

        var requestData = new
        {
            action,
            collection,
            query,
            update_values = data
        };

        var response = await _httpClient.PostAsJsonAsync("/api/persist", requestData);
        return response;
    }

    public async Task<HttpResponseMessage> InsertData(string collectionName, object document)
    {
        var requestData = new
        {
            action = "insert",
            collection = collectionName,
            document
        };

        return await _httpClient.PostAsJsonAsync("/api/persist", requestData);
    }

    public async Task<HttpResponseMessage> FetchClosingPrice(string date, string ticker)
    {
        var data = new { date, ticker };
        return await _httpClient.PostAsJsonAsync("/api/closingPrice", data);
    }

    public async Task<Accuracy> GetAccuracy(List<HistoryData> results)
    {
        var accuracy = new Accuracy();
        if (results != null && results.Count > 0)
        {
            var counts = await GetCounts(results);
            var totalCorrect = counts.BullishCorrect + counts.BearishCorrect;

            accuracy.BullishAccuracy = $"{counts.BullishCorrect}/{counts.BullishTotal}";
            accuracy.BearishAccuracy = $"{counts.BearishCorrect}/{counts.BearishTotal}";
            accuracy.TotalAccuracy = counts.TotalPredictions > 0 ? $"{((double)totalCorrect / counts.TotalPredictions * 100):F2}%" : "N/A";
        }
        return accuracy;
    }

    private async Task<PredictionCounts> GetCounts(List<HistoryData> results)
    {
        var counts = new PredictionCounts();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var yesterday = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");

        foreach (var result in results)
        {
            if (result.Date.ToString("yyyy-MM-dd") == today || result.Date.ToString("yyyy-MM-dd") == yesterday)
            {
                continue;
            }
            var response = await FetchClosingPrice(result.Date.ToString("yyyy-MM-dd"), result.ticker);
            if (response.IsSuccessStatusCode)
            {
                var closingPriceResponse = await response.Content.ReadFromJsonAsync<ClosingPriceResponse>();
                var price = closingPriceResponse.ClosingPrice;
                Console.WriteLine("1");
                Console.WriteLine((decimal)result.closing_price);
                Console.WriteLine("2");
                Console.WriteLine(price);

                var stockDirection = price - (decimal)result.closing_price;
                
                var prediction = result.integrated_output.general_prediction.ToLower().Replace(".", "");
                if (prediction == "bullish")
                {
                    counts.BullishTotal++;
                    counts.TotalPredictions++;
                    if (stockDirection > 0)
                    {
                        counts.BullishCorrect++;
                    }
                }
                else if (prediction == "bearish")
                {
                    counts.BearishTotal++;
                    counts.TotalPredictions++;
                    if (stockDirection < 0)
                    {
                        counts.BearishCorrect++;
                    }
                }
            }
        }
        return counts;
    }

    public async Task<string> GetActualResult(string ticker, DateTime date, string prediction, double historicalClose)
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        if (date == today || date == yesterday || prediction == "n/a")
        {
            return "N/A";
        }
        Console.WriteLine("7");

        var response = await FetchClosingPrice(date.ToString("yyyy-MM-dd"), ticker);
        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonResponse);
                    Console.WriteLine("90");

            if (jsonDocument.RootElement.TryGetProperty("Closing Price", out var actualClosingPriceElement) && actualClosingPriceElement.TryGetDecimal(out var actualClosingPrice))
            {
                Console.WriteLine("91");
                var direction = actualClosingPrice - (decimal)historicalClose;

                if (direction > 0) return "Bullish";
                if (direction < 0) return "Bearish";
                return "N/A";
            }
        }

        return "N/A";
    }
}

public class PredictionCounts
{
    public int BullishCorrect { get; set; }
    public int BullishTotal { get; set; }
    public int BearishCorrect { get; set; }
    public int BearishTotal { get; set; }
    public int TotalPredictions { get; set; }
}

public class Accuracy
{
    public string BullishAccuracy { get; set; }
    public string BearishAccuracy { get; set; }
    public string TotalAccuracy { get; set; }
}

public class ClosingPriceResponse
{
    public string Ticker { get; set; }
    public string Date { get; set; }

    [JsonPropertyName("Closing Price")]
    public decimal ClosingPrice { get; set; }
}