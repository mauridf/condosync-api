using Microsoft.EntityFrameworkCore;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data;

public class AdminDbContext : DbContext
{
    public DbSet<SuperAdmin> SuperAdmins { get; set; } = null!;

    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("admin");

        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.ToTable("super_admins");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(30)
                .HasConversion<string>()
                .HasDefaultValue("super_admin");

            entity.Property(e => e.FailedLoginAttempts)
                .HasDefaultValue(0);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.TwoFactorEnabled)
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW()");
        });
    }
}