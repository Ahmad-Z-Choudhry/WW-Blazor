using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyBlazorApp;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Create an HttpClient instance to fetch appsettings.json and proxy.config.json
var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

// Fetch the appsettings.json file
var response = await httpClient.GetAsync("appsettings.json");
response.EnsureSuccessStatusCode();
var json = await response.Content.ReadAsStringAsync();

// Fetch the proxy.config.json file
var proxyResponse = await httpClient.GetAsync("proxy.config.json");
proxyResponse.EnsureSuccessStatusCode();
var proxyJson = await proxyResponse.Content.ReadAsStringAsync();

// Parse the JSON to a configuration object
var configuration = new ConfigurationBuilder()
    .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
    .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(proxyJson)))
    .Build();

// Register HttpClient with the base address from proxy configuration
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:3000/api") });
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<TickerService>();

await builder.Build().RunAsync();
