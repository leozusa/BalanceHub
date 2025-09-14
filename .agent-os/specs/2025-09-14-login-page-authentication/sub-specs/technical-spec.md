# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-09-14-login-page-authentication/spec.md

## Prerequisites

### Project Scaffold
1. **Angular 16 Project Setup**
   - Initialize new Angular workspace with Angular CLI
   - Install required packages: @angular/core, @angular/forms, @angular/router, @angular/common
   - Configure PrimeNG and PrimeIcons integration

2. **C# ASP.NET Core API Project Setup**
   - Create new ASP.NET Core Web API project (.NET 8.0)
   - Install NuGet packages: Microsoft.AspNetCore.Authentication.JwtBearer, EntityFrameworkCore.SqlServer
   - Configure CORS for frontend communication

3. **Azure Infrastructure Setup**
   - Configure Azure Static Web Apps for frontend hosting
   - Set up Azure SQL Database with connection strings
   - Create Microsoft Entra ID application for OAuth

## Technical Requirements

### Frontend (Angular)

- **Component Architecture**: Create LoginComponent with template, styles, and TypeScript logic
- **Reactive Forms**: Use FormBuilder with Validators for email/password with custom validators
- **OAuth Integration**: Implement PKCE flow for Microsoft Entra ID using Angular's HttpClient
- **Routing Guards**: Create AuthGuard service to protect routes and handle JWT validation
- **Token Management**: Implement TokenService for JWT storage in localStorage/sessionStorage
- **Role-Based Navigation**: Use Angular Router for conditional redirect based on user role from JWT claims
- **Error Handling**: Display PrimeNG Messages/Toast components for authentication errors
- **Responsive Design**: Use CSS Grid/Flexbox with PrimeNG responsive classes
- **Loading States**: Implement loading spinner during authentication requests
- **Accessibility**: Add ARIA labels and keyboard navigation support

### Backend (C# ASP.NET Core)

- **Authentication Controller**: Create AuthController with Login endpoint accepting email/password
- **OAuth Middleware**: Configure Microsoft Entra ID integration in Startup/Program.cs
- **JWT Token Service**: Implement ITokenService interface for JWT generation with role claims
- **User Validation**: Use Entity Framework to query Users table for email/password verification
- **Security Headers**: Add middleware for HTTPS redirection and security headers
- **CORS Configuration**: Enable cross-origin requests from Angular frontend
- **Logging**: Implement structured logging for authentication attempts (without sensitive data)

### Database Schema (Preview)

- **Users Table**: Created separately in database-schema.md
- **Connection Strategy**: Azure SQL Database with connection pooling
- **Migration Scripts**: Entity Framework migrations for schema deployment

### Security Considerations

- **HTTPS Enforcement**: Require secure connections for all authentication endpoints
- **Rate Limiting**: Implement middleware to prevent brute-force attacks
- **Input Sanitization**: Validate and sanitize all user inputs
- **Token Expiration**: Set JWT expiration times and refresh token capabilities
- **Session Management**: Secure token storage with HttpOnly cookies option

### Performance Requirements

- **Login API Response**: < 2 seconds average response time
- **OAuth Redirect**: < 3 seconds for full OAuth flow completion
- **Frontend Bundle Size**: Maintain below 1MB until optimization phase
- **Database Query**: < 500ms for user credential verification

### Integration Points

- **Azure Static Web Apps**: Frontend deployment with API backend integration
- **Azure SQL Database**: User data persistence with Entity Framework
- **Microsoft Entra ID**: OAuth provider for manager authentication
- **Azure Application Insights**: Monitoring and logging for authentication events

## Testing Requirements

- **Unit Tests**: Create xUnit tests for authentication logic and services
- **Component Tests**: Angular test framework for login component behavior
- **Integration Tests**: End-to-end login flow with OAuth simulation
- **Security Testing**: Validate against common authentication vulnerabilities
