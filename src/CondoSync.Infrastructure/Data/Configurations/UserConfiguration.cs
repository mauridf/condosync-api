using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;

namespace CondoSync.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.CondominiumId)
            .IsRequired();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.Cpf)
            .HasMaxLength(14);

        builder.Property(e => e.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(UserRole.Resident);

        builder.Property(e => e.FailedLoginAttempts)
            .HasDefaultValue(0);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.TwoFactorEnabled)
            .HasDefaultValue(false);

        builder.Property(e => e.TwoFactorSecret)
            .HasMaxLength(100);

        builder.Property(e => e.NotificationPreferences)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{\"email\": true, \"push\": true, \"in_app\": true}'::jsonb");

        builder.Property(e => e.ThemePreferences)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{\"mode\": \"light\", \"accent_color\": \"#1976D2\"}'::jsonb");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Índices
        builder.HasIndex(e => new { e.CondominiumId, e.Email })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        builder.HasIndex(e => new { e.CondominiumId, e.Role });

        builder.HasIndex(e => e.Email)
            .HasFilter("email IS NOT NULL");

        // Relacionamento
        builder.HasOne<Condominium>()
            .WithMany()
            .HasForeignKey(e => e.CondominiumId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete filter
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}