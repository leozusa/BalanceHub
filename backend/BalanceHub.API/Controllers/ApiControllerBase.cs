using Microsoft.AspNetCore.Mvc;

namespace BalanceHub.API.Controllers;

/// <summary>
/// Base controller that provides common functionality for all API controllers.
/// This establishes consistency across all endpoints and provides standardized
/// behavior for things like content-type validation and error handling.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Provides access to the HTTP context for derived controllers.
    /// This can be used to access request headers, user information, etc.
    /// </summary>
    protected IHttpContextAccessor? HttpContextAccessor { get; }

    /// <summary>
    /// Called to validate the current request.
    /// Override this method in derived controllers to add custom validation logic.
    /// </summary>
    /// <returns>True if the request is valid, false otherwise</returns>
    protected virtual bool ValidateRequest()
    {
        return ModelState.IsValid;
    }

    /// <summary>
    /// Creates a standardized success response with optional data.
    /// This ensures consistent response format across all endpoints.
    /// </summary>
    /// <param name="data">The data to return (optional)</param>
    /// <param name="message">Optional success message</param>
    /// <returns>Standardized success response</returns>
    protected IActionResult SuccessResponse(object? data = null, string? message = null)
    {
        return Ok(new
        {
            success = true,
            message = message ?? "Operation completed successfully",
            data = data,
            timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Creates a standardized error response with consistent format.
    /// This ensures all error responses follow the same structure.
    /// </summary>
    /// <param name="message">Error message describing what went wrong</param>
    /// <param name="statusCode">HTTP status code (defaults to 400 Bad Request)</param>
    /// <returns>Standardized error response</returns>
    protected IActionResult ErrorResponse(string message, int statusCode = 400)
    {
        return StatusCode(statusCode, new
        {
            success = false,
            message = message,
            timestamp = DateTimeOffset.UtcNow,
            path = Request.Path.Value,
            method = Request.Method
        });
    }

    /// <summary>
    /// Validates if the current request has proper authorization.
    /// This is a base implementation that can be overridden for specific requirements.
    /// </summary>
    /// <returns>True if the request is properly authorized</returns>
    protected virtual bool IsAuthorized()
    {
        return HttpContext.User.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Logs an information message with standard formatting.
    /// This provides consistent logging across all controllers.
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="args">Optional formatting arguments</param>
    protected void LogInformation(string message, params object[] args)
    {
        var logger = HttpContext.RequestServices.GetService<ILogger<ApiControllerBase>>();
        logger?.LogInformation(message, args);
    }

    /// <summary>
    /// Logs a warning message with standard formatting.
    /// </summary>
    /// <param name="message">The warning message</param>
    /// <param name="args">Optional formatting arguments</param>
    protected void LogWarning(string message, params object[] args)
    {
        var logger = HttpContext.RequestServices.GetService<ILogger<ApiControllerBase>>();
        logger?.LogWarning(message, args);
    }

    /// <summary>
    /// Logs an error message with standard formatting.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="message">Additional context message</param>
    /// <param name="args">Optional formatting arguments</param>
    protected void LogError(Exception exception, string message, params object[] args)
    {
        var logger = HttpContext.RequestServices.GetService<ILogger<ApiControllerBase>>();
        logger?.LogError(exception, message, args);
    }
}
