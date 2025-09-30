using System.ComponentModel.DataAnnotations;

namespace BalanceHub.API.Models;

public class User
{
    public Guid Id { get; set; }

    [EmailAddress]
    [Required]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Employee";

    [MaxLength(500)]
    public string? EntraId { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    // Security properties for account lockout
    public int FailedLoginAttempts { get; set; } = 0;

    public DateTime? LockoutEnd { get; set; }

    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    // Calculated properties
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string FullName => $"{FirstName ?? string.Empty} {LastName ?? string.Empty}".Trim();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsManager => Role == "Manager";
}
