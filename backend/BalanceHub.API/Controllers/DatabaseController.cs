using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BalanceHub.API.Data;
using BalanceHub.API.Models;

namespace BalanceHub.API.Controllers;

[ApiController]
[Route("api/database")]
public class DatabaseController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(ApplicationDbContext context, ILogger<DatabaseController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeDatabase()
    {
        try
        {
            _logger.LogInformation("Starting simple database initialization...");

            // Use Entity Framework Core to create the database and tables
            await _context.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database and tables ensured.");

            // Check if users already exist
            if (await _context.Users.AnyAsync())
            {
                return Ok(new { message = "Database already has users", status = "already_initialized" });
            }

            // Add test users directly using EF
            var testUsers = new[]
            {
                new User
                {
                    Email = "john.doe@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    Role = "Employee",
                    IsActive = true
                },
                new User
                {
                    Email = "sarah.smith@example.com",
                    FirstName = "Sarah",
                    LastName = "Smith",
                    Role = "Manager",
                    IsActive = true
                },
                new User
                {
                    Email = "alex.jones@example.com",
                    FirstName = "Alex",
                    LastName = "Jones",
                    Role = "Employee",
                    IsActive = true
                }
            };

            await _context.Users.AddRangeAsync(testUsers);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added {Count} test users", testUsers.Length);

            return Ok(new
            {
                message = "Database initialized successfully with 3 test users",
                status = "success",
                userCount = testUsers.Length,
                testCredentials = testUsers.Select(u => new
                {
                    Email = u.Email,
                    Role = u.Role,
                    Password = "test123"
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing database: {Message}", ex.Message);

            // Try a fallback SQL approach
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                var checkTableCommand = connection.CreateCommand();
                checkTableCommand.CommandText = @"
                    IF EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                    BEGIN
                        IF EXISTS (SELECT * FROM [dbo].[Users] WHERE Email = 'john.doe@example.com')
                        BEGIN
                            SELECT 'users_exist'
                        END
                        ELSE
                        BEGIN
                            INSERT INTO [dbo].[Users] ([Id], [Email], [FirstName], [LastName], [Role], [IsActive])
                            SELECT NEWID(), 'john.doe@example.com', 'John', 'Doe', 'Employee', 1
                            UNION ALL
                            SELECT NEWID(), 'sarah.smith@example.com', 'Sarah', 'Smith', 'Manager', 1
                            UNION ALL
                            SELECT NEWID(), 'alex.jones@example.com', 'Alex', 'Jones', 'Employee', 1;
                            SELECT 'users_added'
                        END
                    END
                    ELSE
                    BEGIN
                        CREATE TABLE [dbo].[Users] (
                            [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                            [Email] NVARCHAR(320) NOT NULL UNIQUE,
                            [PasswordHash] NVARCHAR(MAX),
                            [Role] NVARCHAR(20) NOT NULL DEFAULT 'Employee',
                            [EntraId] NVARCHAR(500),
                            [FirstName] NVARCHAR(100),
                            [LastName] NVARCHAR(100),
                            [IsActive] BIT NOT NULL DEFAULT 1,
                            [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            [LastLoginAt] DATETIME2
                        );

                        INSERT INTO [dbo].[Users] ([Id], [Email], [FirstName], [LastName], [Role], [IsActive])
                        SELECT NEWID(), 'john.doe@example.com', 'John', 'Doe', 'Employee', 1
                        UNION ALL
                        SELECT NEWID(), 'sarah.smith@example.com', 'Sarah', 'Smith', 'Manager', 1
                        UNION ALL
                        SELECT NEWID(), 'alex.jones@example.com', 'Alex', 'Jones', 'Employee', 1;

                        SELECT 'table_created'
                    END";

                var result = await checkTableCommand.ExecuteScalarAsync();
                var resultString = result?.ToString() ?? "unknown";

                await connection.CloseAsync();

                if (resultString == "users_exist")
                {
                    return Ok(new { message = "Users already exist", status = "already_initialized" });
                }

                return Ok(new
                {
                    message = $"Database initialized - {resultString}",
                    status = "success",
                    testCredentials = new[]
                    {
                        new { Email = "john.doe@example.com", Password = "test123", Role = "Employee" },
                        new { Email = "sarah.smith@example.com", Password = "test123", Role = "Manager" },
                        new { Email = "alex.jones@example.com", Password = "test123", Role = "Employee" }
                    }
                });
            }
            catch (Exception sqlEx)
            {
                _logger.LogError(sqlEx, "SQL fallback also failed: {Message}", sqlEx.Message);
                return StatusCode(500, new
                {
                    message = $"Both EF and SQL initialization failed. EF: {ex.Message}, SQL: {sqlEx.Message}",
                    status = "error"
                });
            }
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            var userCount = await _context.Users.CountAsync();

            var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync()).ToList();

            return Ok(new
            {
                databaseConnected = canConnect,
                usersCount = userCount,
                pendingMigrations = pendingMigrations,
                status = canConnect ? "healthy" : "disconnected"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                databaseConnected = false,
                error = ex.Message,
                status = "error"
            });
        }
    }
}
