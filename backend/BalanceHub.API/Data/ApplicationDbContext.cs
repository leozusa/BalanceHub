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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=BalanceHub.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}
