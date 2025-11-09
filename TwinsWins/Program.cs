using Fluxor;
using Fluxor.Blazor.Web.ReduxDevTools;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TwinsWins.Client;

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

// Add Fluxor for state management with proper configuration
builder.Services.AddFluxor(options =>
{
    options
        .ScanAssemblies(typeof(Program).Assembly)
#if DEBUG
        .UseReduxDevTools() // Enable Redux DevTools for debugging
        // Uncomment the above line and add the correct using if you have the Fluxor.Blazor.Web.ReduxDevTools package installed.
        .UseRouting() // Required for Blazor WASM
#endif
        ;
});

Console.WriteLine("TwinsWins Blazor WASM starting...");
Console.WriteLine("Fluxor configured with assembly scanning");

await builder.Build().RunAsync();