using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TwinsWins.Client;
using TwinsWins.Client.Store.Game;
using TwinsWins.Client.Store.Lobby;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for API calls
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7103";
Console.WriteLine($"API Base URL: {apiBaseUrl}");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// Add Fluxor for state management
builder.Services.AddFluxor(options =>
{
    options.ScanAssemblies(typeof(Program).Assembly);
});

// Register Effects manually (Fluxor requires this)
builder.Services.AddScoped<GameEffects>();
builder.Services.AddScoped<LobbyEffects>();

Console.WriteLine("TwinsWins Blazor WASM starting...");
await builder.Build().RunAsync();