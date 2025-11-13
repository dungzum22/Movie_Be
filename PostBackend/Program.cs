using Microsoft.EntityFrameworkCore;
using PostBackend.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure port for Render (use PORT env var if set, otherwise default to 8080)
var httpPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(httpPort) && int.TryParse(httpPort, out var portNumber))
{
    builder.WebHost.UseUrls($"http://*:{portNumber}");
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Movie API", Version = "v1" });
    options.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "date" });
    options.MapType<TimeOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time" });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Allow localhost for development
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "https://localhost:3000", "https://localhost:3001")
              .SetIsOriginAllowed(origin =>
              {
                  // Allow any Vercel app domain
                  return origin.Contains("vercel.app") ||
                         origin.StartsWith("http://localhost") ||
                         origin.StartsWith("https://localhost");
              })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// EF Core - PostgreSQL (Npgsql)
// Try to get connection string from environment variable or configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// If not found, try to parse DATABASE_URL (common on Render when database is linked)
if (string.IsNullOrEmpty(connectionString))
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        // Parse postgresql://user:password@host:port/database format
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var port = uri.Port == -1 ? 5432 : uri.Port; // Default to 5432 if port not specified
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = uri.LocalPath.TrimStart('/');
        
        connectionString = $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password}";
    }
}

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Please set ConnectionStrings__DefaultConnection or DATABASE_URL environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Apply any pending EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Create uploads folder for static files
var env = app.Services.GetRequiredService<IWebHostEnvironment>();
var webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads"));

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

// Only use HTTPS redirection in development (Render handles HTTPS at load balancer)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
