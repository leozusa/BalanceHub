using Microsoft.AspNetCore.Mvc;

namespace BalanceHub.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new {
            message = "BalanceHub authentication API is live!",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            status = "AUTHENTICATION ACTIVE"
        });
    }

    [HttpPost("auth/login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Mock authentication for testing
        return Ok(new {
            token = $"jwt-{Guid.NewGuid().ToString()}",
            user = new {
                email = request?.Email ?? "test@example.com",
                role = "Employee"
            },
            expiresIn = 3600
        });
    }
}

public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
}
