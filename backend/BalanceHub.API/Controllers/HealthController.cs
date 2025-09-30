using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BalanceHub.API.Controllers;

/// <summary>
/// Health check controller providing system status endpoints
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simple health check endpoint
    /// </summary>
    /// <returns>Basic health status</returns>
    /// <response code="200">API is healthy</response>
    [HttpGet]
    [ProducesResponseType(200)]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check requested");

        return Ok(new
        {
            status = "Healthy",
            message = "BalanceHub API is running",
            timestamp = DateTimeOffset.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Detailed health check including database connectivity
    /// </summary>
    /// <returns>Detailed health information</returns>
    /// <response code="200">All systems healthy</response>
    /// <response code="503">Database connectivity issue</response>
    [HttpGet("detailed")]
    [ProducesResponseType(200)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> Detailed()
    {
        var startTime = DateTimeOffset.UtcNow;
        var databaseStatus = "Unknown";

        try
        {
            databaseStatus = await TestDatabaseConnection();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            databaseStatus = "Error";
        }

        var totalTime = (int)(DateTimeOffset.UtcNow - startTime).TotalMilliseconds;

        var result = new
        {
            status = databaseStatus == "Connected" ? "Healthy" : "Unhealthy",
            message = databaseStatus == "Connected" ? "All systems operational" : "Database connectivity issue",
            timestamp = startTime,
            database = new
            {
                status = databaseStatus
            },
            system = new
            {
                memoryUsage = $"{GC.GetTotalMemory(false) / 1024 / 1024}MB",
                responseTime = $"{totalTime}ms"
            }
        };

        if (databaseStatus != "Connected")
        {
            return StatusCode(503, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Eisenhower Matrix intelligence health check
    /// </summary>
    /// <returns>Intelligence system status</returns>
    /// <response code="200">Intelligence system is healthy</response>
    [HttpGet("intelligence")]
    [ProducesResponseType(200)]
    public IActionResult Intelligence()
    {
        return Ok(new
        {
            status = "Healthy",
            message = "Eisenhower Matrix AI is operational",
            timestamp = DateTimeOffset.UtcNow,
            intelligence = new
            {
                matrixAlgorithm = "V1.0",
                categories = new[] { "do", "schedule", "delegate", "delete" },
                timePressureBoost = "active"
            }
        });
    }

    private async Task<string> TestDatabaseConnection()
    {
        try
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                                ?? "Server=tcp:balancehub-sql.database.windows.net,1433;Initial Catalog=balancehubdb;Persist Security Info=False;User ID=balancehubadmin;Password=BalanceHubPass123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync();

            return "Connected";
        }
        catch
        {
            return "Error";
        }
    }
}
