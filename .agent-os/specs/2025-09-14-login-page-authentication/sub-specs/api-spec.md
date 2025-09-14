# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-09-14-login-page-authentication/spec.md

## Endpoints

### POST /api/auth/login

**Purpose:** Authenticate user via email/password combination and issue secure JWT token for session management.

**Parameters:**
- **Content-Type**: application/json
- **Body:**
```json
{
  "email": "user@company.com",
  "password": "string",
  "rememberMe": "boolean"
}
```

**Response (200 OK):**
```json
{
  "token": "jwt.access.token.here",
  "user": {
    "id": "uuid-string",
    "email": "user@company.com",
    "role": "Employee",
    "firstName": "John",
    "lastName": "Doe"
  },
  "expiresIn": 3600
}
```

**Errors:**
- **400 Bad Request**: Invalid email format or missing required fields
- **401 Unauthorized**: Invalid credentials or inactive account
- **429 Too Many Requests**: Rate limit exceeded (brute force prevention)
- **500 Internal Server Error**: Server-side processing error

## Controllers

### AuthController

**Location:** Controllers/AuthController.cs

**Actions:**

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Method signature for email/password authentication
    }
}
```

**Request Model:**
```csharp
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = default!;

    public bool RememberMe { get; set; } = false;
}
```

**Response Model:**
```csharp
public class LoginResponse
{
    public string Token { get; set; } = default!;
    public UserSummaryResponse User { get; set; } = default!;
    public int ExpiresIn { get; set; } = 3600; // seconds
}
```

**Business Logic:**
1. Validate input model and sanitize email
2. Query Users table by email (case-insensitive search)
3. Verify account is active and not locked
4. Compare password against stored hash using secure algorithm (e.g., bcrypt)
5. Generate JWT with user claims (Id, Email, Role)
6. Set appropriate expiration based on RememberMe flag
7. Update LastLoginAt timestamp in database
8. Return success response or appropriate error

**Error Handling:**
- **Invalid Credentials**: Generic "Invalid email or password" message (security)
- **Account Disabled**: "Account is currently disabled" with contact instruction
- **Validation Errors**: Detailed field-by-field validation messages
- **Server Errors**: Generic "Authentication service unavailable" message with reference ID

## Purpose

### Endpoint Rationale

- **Single Authentication Entry Point**: Unified login endpoint reduces complexity and provides consistent API contract
- **Secure Token Generation**: JWT creation with proper signing and time-based expiration
- **Role-Based Claims**: Embed user permissions directly in token for frontend authorization
- **Rate Limiting Ready**: Positioning for middleware implementation to prevent abuse
- **Audit Trail**: Login tracking enables security monitoring and user analytics

### Integration with Features

- **JWT Token Flow**: Frontend receives token, stores in localStorage/sessionStorage, includes in all API requests via Authorization header
- **Role-Based Navigation**: Frontend reads role from JWT payload to determine redirect target (timer hub vs manager dashboard)
- **Automatic Token Refresh**: RememberMe flag influences expiration time for smoother user experience
- **Cross-Origin Support**: CORS configured to accept requests from Angular frontend URL

### OAuth Integration Notes

- **Microsoft Entra ID Configuration**: OAuth handled at application level (program.cs/app settings) rather than API endpoint
- **PKCE Flow**: Proof Key for Code Exchange implemented at Angular level to prevent authorization code interception
- **Callback Handling**: OAuth completion creates user record if needed, issues JWT, redirects to appropriate dashboard
- **Dual Authentication**: System supports both traditional OAuth and email/password methods simultaneously

## Security Considerations

- **HTTPS Only**: All authentication endpoints require SSL/TLS
- **Input Validation**: Email regex validation and length limits for all inputs
- **SQL Injection Prevention**: Entity Framework parameterized queries
- **Password Security**: Secure hashing with salt (implemented separately at infrastructure level)
- **Session Management**: JWT contains minimal data, rotated on each login
- **Logging Policy**: Authentication attempts logged without capturing password data

## Performance Targets

- **Response Time**: < 1.5 seconds for successful login operations
- **Concurrent Users**: Support 50+ simultaneous login attempts
- **Database Load**: Optimized query with index on email field
- **Memory Usage**: Token generation doesn't cause memory leaks
