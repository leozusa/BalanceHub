using Microsoft.EntityFrameworkCore;
using BalanceHub.API.Models;

namespace BalanceHub.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext()
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Models.Task> Tasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Use SQLite for development - this will be overridden by production DI configuration
            optionsBuilder.UseSqlite("Data Source=BalanceHub.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User entity configuration
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(320);
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Property(u => u.Role).IsRequired().HasMaxLength(20);
        modelBuilder.Entity<User>().HasIndex(u => u.Role);
        modelBuilder.Entity<User>().Property(u => u.EntraId).HasMaxLength(500);
        modelBuilder.Entity<User>().HasIndex(u => u.EntraId).HasFilter("[EntraId] IS NOT NULL");
        modelBuilder.Entity<User>().Property(u => u.IsActive).HasDefaultValue(true);
        modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        modelBuilder.Entity<User>().Property(u => u.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

        // Task entity configuration
        modelBuilder.Entity<Models.Task>().HasKey(t => t.Id);
        modelBuilder.Entity<Models.Task>().Property(t => t.Title).IsRequired().HasMaxLength(500);
        modelBuilder.Entity<Models.Task>().Property(t => t.Description).HasMaxLength(2000);
        modelBuilder.Entity<Models.Task>().Property(t => t.Urgency).HasDefaultValue(5);
        modelBuilder.Entity<Models.Task>().Property(t => t.Importance).HasDefaultValue(5);
        modelBuilder.Entity<Models.Task>().Property(t => t.MatrixType).IsRequired().HasMaxLength(20).HasDefaultValue("do");
        modelBuilder.Entity<Models.Task>().Property(t => t.CalculatedPriority).HasDefaultValue(5);
        modelBuilder.Entity<Models.Task>().Property(t => t.EstimatedHours).HasDefaultValue(1.0);
        modelBuilder.Entity<Models.Task>().Property(t => t.ActualHours).HasDefaultValue(0.0);
        modelBuilder.Entity<Models.Task>().Property(t => t.EffortLevel).IsRequired().HasMaxLength(20).HasDefaultValue("medium");
        modelBuilder.Entity<Models.Task>().Property(t => t.Status).IsRequired().HasMaxLength(20).HasDefaultValue("todo");
        modelBuilder.Entity<Models.Task>().Property(t => t.Category).HasMaxLength(100);
        modelBuilder.Entity<Models.Task>().Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        modelBuilder.Entity<Models.Task>().Property(t => t.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        modelBuilder.Entity<Models.Task>().Property(t => t.IsDeleted).HasDefaultValue(false);
        modelBuilder.Entity<Models.Task>().Property(t => t.TimePressure).HasDefaultValue(0.0);
        modelBuilder.Entity<Models.Task>().Property(t => t.PriorityDecay).HasDefaultValue(0.0);
        modelBuilder.Entity<Models.Task>().Property(t => t.RescheduleCount).HasDefaultValue(0);

        // Indexes for performance
        modelBuilder.Entity<Models.Task>().HasIndex(t => t.UserId);
        modelBuilder.Entity<Models.Task>().HasIndex(t => t.Status);
        modelBuilder.Entity<Models.Task>().HasIndex(t => t.MatrixType);
        modelBuilder.Entity<Models.Task>().HasIndex(t => t.Category);
        modelBuilder.Entity<Models.Task>().HasIndex(t => t.Deadline);
        modelBuilder.Entity<Models.Task>().HasIndex(t => t.CalculatedPriority);
        modelBuilder.Entity<Models.Task>().HasIndex(t => new { t.UserId, t.IsDeleted }).HasFilter("[IsDeleted] = 0");
        modelBuilder.Entity<Models.Task>().HasIndex(t => new { t.UserId, t.Status }).HasFilter("[IsDeleted] = 0");

        // Foreign key relationship
        modelBuilder.Entity<Models.Task>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
