using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.IdentityModel.Tokens;
using CanteenBackend.Data;
using CanteenBackend.Data.Repositories;
using CanteenBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Controllers + Swagger
// ------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------------------------------------------------
// CORS (required for frontend + SSE)
// ------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Type");
    });
});

// ------------------------------------------------------------
// Database Connection
// ------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var sqlManager = new SqlDataManager();
sqlManager.Connect(connectionString);

builder.Services.AddSingleton(sqlManager);

// ------------------------------------------------------------
// Repositories
// ------------------------------------------------------------
builder.Services.AddSingleton<ScanRepository>();
builder.Services.AddSingleton<PersonRepository>();
builder.Services.AddSingleton<MealRepository>();
builder.Services.AddSingleton<AdminRepository>();

// ------------------------------------------------------------
// Services
// ------------------------------------------------------------
builder.Services.AddSingleton<ScanService>();
builder.Services.AddSingleton<MealSessionState>();
builder.Services.AddSingleton<EventStream>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<AdminService>();

// ------------------------------------------------------------
// JWT Authentication
// ------------------------------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

var app = builder.Build();

// ------------------------------------------------------------
// Log every incoming request
// ------------------------------------------------------------
app.Use(async (context, next) =>
{
    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {context.Request.Method} {context.Request.Path} from {ip}");
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

// CORS must be BEFORE authentication and controllers
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ------------------------------------------------------------
// Log hosting URLs on startup
// ------------------------------------------------------------
app.Lifetime.ApplicationStarted.Register(() =>
{
    var server = app.Services.GetRequiredService<IServer>();
    var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;

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
