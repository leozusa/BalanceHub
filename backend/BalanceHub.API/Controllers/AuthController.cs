using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BalanceHub.API.Data;
using BalanceHub.API.Models;

namespace BalanceHub.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login attempt with missing required fields");
                return BadRequest(new { message = "Email and password are required" });
            }

            // Find user by email (case insensitive)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower())
                .ConfigureAwait(false);

            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
                return Unauthorized(new { message = "Account is inactive" });
            }

            // For now, we'll implement a simple password check
            // In production, this should use proper password hashing like BCrypt
            // Here we're assuming the password has been stored properly during registration
            var isValidPassword = await VerifyPasswordAsync(user, request.Password);

            if (!isValidPassword)
            {
                _logger.LogWarning("Invalid password for user: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user, request.RememberMe);

            var response = new LoginResponse
            {
                Token = token,
                User = new UserSummaryResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
                },
                ExpiresIn = request.RememberMe ? 7 * 24 * 60 * 60 : 60 * 60 // 7 days or 1 hour
            };

            _logger.LogInformation("Successful login for user: {Email}", request.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    // Placeholder for password verification - should be implemented with hashing
    private async Task<bool> VerifyPasswordAsync(User user, string password)
    {
        // For now, accept any password for testing
        // In production, use BCrypt.Verify(password, user.PasswordHash)
        return !string.IsNullOrWhiteSpace(password);
    }

    private string GenerateJwtToken(User user, bool rememberMe)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new Claim("role", user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-super-secret-key-here-change-in-production"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiration = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: "BalanceHub.Api",
            audience: "BalanceHub",
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public bool RememberMe { get; set; } = false;
}

public class LoginResponse
{
    public required string Token { get; set; }
    public required UserSummaryResponse User { get; set; }
    public int ExpiresIn { get; set; }
}

public class UserSummaryResponse
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Role { get; set; }
}
