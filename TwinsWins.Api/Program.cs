using Microsoft.EntityFrameworkCore;
using TwinsWins.Api.Data;
using TwinsWins.Api.Data.Repositories;
using TwinsWins.Api.Services;
using TwinsWins.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ============ SERVICES CONFIGURATION ============

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TwinsWins API",
        Version = "v1",
        Description = "API for TwinsWins memory matching game with TON blockchain integration"
    });
});

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Add CORS - MUST be configured before AddControllers
var clientUrl = builder.Configuration["ClientUrl"] ?? "https://localhost:7236";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7236",  // Blazor WASM HTTPS
                "http://localhost:5151",   // Blazor WASM HTTP  
                clientUrl
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Required for SignalR
    });
});

// Add Database Context
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// Register Repositories
builder.Services.AddScoped<IGameLobbyRepository, GameLobbyRepository>();
builder.Services.AddScoped<IGameTransactionRepository, GameTransactionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register Services
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddSingleton<IImageService>(sp =>
{
    var imageDirectory = builder.Configuration["ImageDirectory"] ?? "wwwroot/images/game";
    var logger = sp.GetService<ILogger<ImageService>>();
    return new ImageService(imageDirectory, logger);
});
builder.Services.AddScoped<ITonWalletService, TonWalletService>();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ============ BUILD APPLICATION ============

var app = builder.Build();

// ============ MIDDLEWARE CONFIGURATION ============

// Configure Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TwinsWins API v1");
        options.RoutePrefix = "swagger";
    });
}

// Use HTTPS redirection
app.UseHttpsRedirection();

// Use CORS (MUST be before UseRouting and after UseHttpsRedirection)
app.UseCors();

// Use routing
app.UseRouting();

// Use authorization (if you add authentication later)
// app.UseAuthentication();
// app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<GameHub>("/gamehub");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    message = "TwinsWins API",
    version = "1.0.0",
    endpoints = new
    {
        swagger = "/swagger",
        health = "/health",
        gameHub = "/gamehub",
        api = "/api"
    }
}));

// ============ DATABASE MIGRATION (Development) ============

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        try
        {
            // Apply any pending migrations
            db.Database.Migrate();
            app.Logger.LogInformation("Database migrated successfully");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Error migrating database");
        }
    }
}

// ============ RUN APPLICATION ============

app.Logger.LogInformation("TwinsWins API starting...");
app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("CORS allowed origins: https://localhost:7236, http://localhost:5151, {ClientUrl}", clientUrl);

app.Run();