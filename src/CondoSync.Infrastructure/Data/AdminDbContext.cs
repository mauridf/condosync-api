using Microsoft.EntityFrameworkCore;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Events;

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

        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.ToTable("super_admins");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(e => e.Role)
                .HasColumnName("role")
                .IsRequired()
                .HasMaxLength(30)
                .HasConversion<string>()
                .HasDefaultValue(SuperAdminRole.SuperAdmin);

            entity.Property(e => e.FailedLoginAttempts)
                .HasColumnName("failed_login_attempts")
                .HasDefaultValue(0);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.TwoFactorEnabled)
                .HasColumnName("two_factor_enabled")
                .HasDefaultValue(false);

            entity.Property(e => e.EmailVerifiedAt)
                .HasColumnName("email_verified_at");

            entity.Property(e => e.LastLoginAt)
                .HasColumnName("last_login_at");

            entity.Property(e => e.LastPasswordChangeAt)
                .HasColumnName("last_password_change_at");

            entity.Property(e => e.LockedUntil)
                .HasColumnName("locked_until");

            entity.Property(e => e.TwoFactorSecret)
                .HasColumnName("two_factor_secret")
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()");
        });
    }
}