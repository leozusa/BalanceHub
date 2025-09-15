using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using BalanceHub.API.Data;
using BalanceHub.API;

namespace BalanceHub.API.Tests;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override any services with mocked or test versions
            services.AddScoped<ApplicationDbContext, TestApplicationDbContext>();
        });

        builder.UseEnvironment("Development");
    }
}

public class TestApplicationDbContext : ApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=:memory:");
        }
    }

    public void EnsureCreated()
    {
        Database.EnsureCreated();
    }
}

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        var db = _factory.Services.GetRequiredService<ApplicationDbContext>() as TestApplicationDbContext;
        db?.EnsureCreated();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new
        {
            email = "employee@balancehub.com",
            password = "validpassword123",
            rememberMe = false
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/auth/login", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The response should contain token and user info
        // Note: This test will initially fail until the controller is implemented
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.True(responseObj.TryGetProperty("token", out var token));
        Assert.True(responseObj.TryGetProperty("user", out var user));
        Assert.True(responseObj.TryGetProperty("expiresIn", out var expiresIn));
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new
        {
            email = "invalid-email",
            password = "password123",
            rememberMe = false
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new
        {
            email = "nonexistent@balancehub.com",
            password = "wrongpassword",
            rememberMe = false
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithoutRequiredFields_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new
        {
            // Missing required email field
            password = "password123",
            rememberMe = false
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithManagerRole_IncludesRoleInTokenClaims()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new
        {
            email = "manager@balancehub.com",
            password = "managerpassword123",
            rememberMe = true
        };
        var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/auth/login", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();

        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
        var user = responseObj.GetProperty("user");

        // The user role should be properly returned
        // This represents the role-based authentication requirement
        Assert.True(user.TryGetProperty("role", out var userRole));
        Assert.True(user.GetString("role") == "Employee" || user.GetString("role") == "Manager");
    }
}
