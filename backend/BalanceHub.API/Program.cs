using BalanceHub.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();                      // Enable MVC Controllers
builder.Services.AddEndpointsApiExplorer();            // Enable API Explorer for Swagger
builder.Services.AddSwaggerGen(options =>               // Add Swagger generator
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BalanceHub - AI Task Management API",
        Version = "v1",
        Description = "Eisenhower Matrix powered productivity intelligence with real-time AI prioritization\n\n## ⚠️ Authentication Required\n\nMost endpoints require a JWT token. Get it from POST /api/auth/login first, then click 'Authorize' button above.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "BalanceHub Team",
            Url = new Uri("https://github.com/leozusa/BalanceHub")
        }
    });

    // Simple authentication note for manual setup
    options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
    {
        Url = "https://balancehub-backend.whitebeach-2c3d67ea.eastus2.azurecontainerapps.io",
        Description = "Production API"
    });
});

// Add application services for authentication and database
builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (connectionString?.Contains("balancehub-sql.database.windows.net") == true)
        {
            options.UseSqlServer(connectionString)
                   .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
        }
        else
        {
            options.UseSqlite(connectionString ?? "Data Source=BalanceHub.db");
        }
    });

builder.Services.AddAuthentication("Bearer")           // Configure JWT Bearer Authentication
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "BalanceHub.Api",
            ValidAudience = "BalanceHub",
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", builder =>
    {
        // Allow specific origins for development
        builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();                                       // Enable Swagger endpoint
app.UseSwaggerUI(options =>                              // Enable Swagger UI
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "BalanceHub AI Task Management API v1");
    options.RoutePrefix = "swagger";                     // Access via /swagger
    options.DocumentTitle = "BalanceHub - Eisenhower Matrix Intelligence API";
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);    // Expand operations by default
    options.DefaultModelsExpandDepth(-1);               // Hide model schemas by default
    options.DisplayRequestDuration();                    // Show request duration
});

app.UseCors("AllowAngular");  // Add CORS middleware before other middleware
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapControllers();  // Map MVC controllers
app.UseAuthentication();  // Add authentication middleware
app.UseAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
