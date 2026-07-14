using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TrackerBackend.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // от циклических ссылок
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        // null-значения
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    // Railway даёт DATABASE_URL
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    var connectionString = !string.IsNullOrEmpty(databaseUrl)
        ? ConvertRailwayUrl(databaseUrl)
        : configuration.GetConnectionString("DefaultConnection");

    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
});

// CORS
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173";

var allowedOrigins = new[]
{
    "http://localhost:5173",
    frontendUrl
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();

    if (!dbContext.Categories.Any())
    {
        var food = new TrackerBackend.Models.Category { Name = "Продукты", MonthlyBudget = 30000, IsActive = true };
        var transport = new TrackerBackend.Models.Category { Name = "Транспорт", MonthlyBudget = 10000, IsActive = true };
        dbContext.Categories.AddRange(food, transport);
        dbContext.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseCors("VueClient");
app.UseAuthorization();

app.MapControllers();

app.Run();

// Конвертер Railway DATABASE_URL
string ConvertRailwayUrl(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');
    return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
