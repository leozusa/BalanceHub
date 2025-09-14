# Database Schema

This is the database schema implementation for the spec detailed in @.agent-os/specs/2025-09-14-login-page-authentication/spec.md

## Schema Changes

### New Table: Users

**SQL DDL:**
```sql
CREATE TABLE [dbo].[Users] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Email] NVARCHAR(320) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(MAX) NULL,
    [Role] NVARCHAR(20) NOT NULL CHECK ([Role] IN ('Employee', 'Manager')),
    [EntraId] NVARCHAR(MAX) NULL,
    [FirstName] NVARCHAR(100) NULL,
    [LastName] NVARCHAR(100) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LastLoginAt] DATETIME2 NULL
);

-- Index for email lookups during authentication
CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users] ([Email]) WHERE [IsActive] = 1;

-- Index for role-based queries (manager analytics)
CREATE NONCLUSTERED INDEX [IX_Users_Role] ON [dbo].[Users] ([Role]) WHERE [IsActive] = 1;

-- Index for Entra ID users
CREATE NONCLUSTERED INDEX [IX_Users_EntraId] ON [dbo].[Users] ([EntraId]) WHERE [EntraId] IS NOT NULL AND [IsActive] = 1;
```

**EF Core Migration:**
```csharp
public partial class AddUsersTableForAuthentication : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Email = table.Column<string>(maxLength: 320, nullable: false),
                PasswordHash = table.Column<string>(nullable: true),
                Role = table.Column<string>(maxLength: 20, nullable: false),
                EntraId = table.Column<string>(nullable: true),
                FirstName = table.Column<string>(maxLength: 100, nullable: true),
                LastName = table.Column<string>(maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                LastLoginAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
                table.CheckConstraint("CK_Users_Role", "Role IN ('Employee', 'Manager')");
                table.UniqueConstraint("UQ_Users_Email", x => x.Email);
            });

        // Indexes
        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Role",
            table: "Users",
            column: "Role");

        migrationBuilder.CreateIndex(
            name: "IX_Users_EntraId",
            table: "Users",
            column: "EntraId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Users");
    }
}
```

## Specification Details

### Entity Framework Model
```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Role { get; set; } = "Employee"; // Employee, Manager
    public string? EntraId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Calculated properties
    [NotMapped]
    public string FullName => $"{FirstName ?? string.Empty} {LastName ?? string.Empty}".Trim();
    [NotMapped]
    public bool IsManager => Role == "Manager";
}
```

### Foreign Key Relationships (Future Extensions)
- Future linkage to feedback tables (for owner/manager relationship)
- Future linkage to task tables (for creator relationships)
- Future linkage to burnout prediction outputs

## Rationale

### Reason for Each Change
- **Users Table**: Core authentication entity storing credentials and identity information
- **Email Unique Constraint**: Prevents duplicate accounts and ensures consistent authentication
- **Role ENUM**: Enforces data integrity with predefined roles for access control
- **EntraId Nullable**: Supports OAuth users who may not have direct password setup
- **PasswordHash Nullable**: Allows OAuth-only users without password storage
- **IsActive Flag**: Soft delete mechanism for account deactivation vs permanent deletion
- **Timestamps**: Auditing capabilities for creation and update tracking
- **LastLoginAt**: Analytics for user engagement and inactive account management

### Performance Considerations
- **Non-Clustered Indexes**: Email is primary lookup key for authentication performance
- **Selective Index Filtering**: IsActive filter reduces index scans on deactivated users
- **Separate Role Index**: Efficient queries for manager-only dashboards
- **Flexible Data Types**: NVARCHAR(MAX) for EntraId to accommodate various OAuth identifiers

### Data Integrity Rules
- **Email Validation**: Application-level regex for RFC-compliant email formats
- **Role Check Constraint**: Database-level enforcement of valid roles
- **Not Null Requirements**: Mandatory fields (Email, Role, Id) prevent incomplete records
- **Timestamp Defaults**: Consistent UTC datetime tracking across all environments

## Migration Strategy

1. **Azure SQL Schema Deployment**: Execute EF migration scripts in staging environment first
2. **Data Seeding**: Create initial test users for development and staging environments
3. **Backup Verification**: Ensure database backup strategy includes Users table
4. **Rollback Plan**: Include Down() method for safe migration reversals
5. **Operations Team Approval**: Security review required before production deployment
