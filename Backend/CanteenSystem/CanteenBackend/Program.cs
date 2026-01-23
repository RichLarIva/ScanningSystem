using CanteenBackend.Data;
using CanteenBackend.Data.Repositories;
using CanteenBackend.Services;
using Microsoft.AspNetCore.Hosting.Server.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------------------------------------------------
// Database Connection Setup
// ------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var sqlManager = new SqlDataManager();
sqlManager.Connect(connectionString);

builder.Services.AddSingleton(sqlManager);

// ------------------------------------------------------------
// Repository Registrations
// ------------------------------------------------------------
builder.Services.AddSingleton<ScanRepository>();
builder.Services.AddSingleton<PersonRepository>();
builder.Services.AddSingleton<MealRepository>();
builder.Services.AddSingleton<AdminRepository>();

// ------------------------------------------------------------
// Service Registrations
// ------------------------------------------------------------
builder.Services.AddSingleton<ScanService>();
builder.Services.AddSingleton<MealSessionState>();
builder.Services.AddSingleton<EventStream>();
builder.Services.AddSingleton<AdminService>();

var app = builder.Build();

// ------------------------------------------------------------
// Log every incoming request
// ------------------------------------------------------------
app.Use(async (context, next) =>
{
    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Incoming {context.Request.Method} {context.Request.Path} from {ip}");
    await next();
});

// ------------------------------------------------------------
// HTTP Request Pipeline
// ------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ------------------------------------------------------------
// Log hosting URLs on startup
// ------------------------------------------------------------
app.Lifetime.ApplicationStarted.Register(() =>
{
    var server = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
    var addresses = server.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()?.Addresses;

    Console.WriteLine("========================================");
    Console.WriteLine(" Canteen Backend Started");
    Console.WriteLine(" Listening on:");

    if (addresses != null)
    {
        foreach (var addr in addresses)
        {
            Console.WriteLine($"  → {addr}");
        }
    }
    else
    {
        Console.WriteLine("  (No addresses found — Kestrel may be using defaults)");
    }

    Console.WriteLine("========================================");
});


app.Run();
