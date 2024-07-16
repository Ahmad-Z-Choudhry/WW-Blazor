using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class TickerService
{
    private readonly HttpClient _httpClient;
    public List<string> Tickers { get; private set; }

    public event Action OnTickersUpdated;

    public TickerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task FetchTickers(string username)
    {
        var data = new
        {
            action = "get_tickers",
            username
        };

        var response = await _httpClient.PostAsJsonAsync("/api/portfolio", data);
        if (response.IsSuccessStatusCode)
        {
            Tickers = await response.Content.ReadFromJsonAsync<List<string>>();
            OnTickersUpdated?.Invoke();
        }
    }

    public async Task AddTicker(string ticker, string username)
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
            await FetchTickers(username); // Refresh tickers after adding
        }
    }

    public async Task RemoveTicker(string ticker, string username)
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
            await FetchTickers(username); // Refresh tickers after removing
        }
    }
}


