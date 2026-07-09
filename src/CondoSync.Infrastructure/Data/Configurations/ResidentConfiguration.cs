// Arquivo: src/CondoSync.Infrastructure/Data/Configurations/ResidentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class ResidentConfiguration : IEntityTypeConfiguration<Resident>
{
    public void Configure(EntityTypeBuilder<Resident> builder)
    {
        builder.ToTable("residents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.UnitId).IsRequired();
        builder.Property(e => e.ResidentType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Cpf).HasMaxLength(14);
        builder.Property(e => e.Rg).HasMaxLength(20);
        builder.Property(e => e.Profession).HasMaxLength(100);
        builder.Property(e => e.OwnerName).HasMaxLength(200);
        builder.Property(e => e.OwnerPhone).HasMaxLength(20);
        builder.Property(e => e.OwnerEmail).HasMaxLength(200);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
        builder.Property(e => e.IsPrimary).HasDefaultValue(false);
        builder.Property(e => e.IsEmergencyContact).HasDefaultValue(false);
        builder.Property(e => e.Vehicles).HasColumnType("jsonb");
        builder.Property(e => e.Pets).HasColumnType("jsonb");
        builder.Property(e => e.HasSystemAccess).HasDefaultValue(false);
        builder.Property(e => e.AccessCode).HasMaxLength(10);
        builder.Property(e => e.BiometricHash).HasMaxLength(200);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.UnitId).HasFilter("deleted_at IS NULL");
        builder.HasIndex(e => e.CondominiumId);
        builder.HasIndex(e => e.Cpf).HasFilter("cpf IS NOT NULL");
        builder.HasIndex(e => e.UnitId).HasFilter("is_primary = true AND deleted_at IS NULL").IsUnique();

        builder.HasOne<Unit>().WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Condominium>().WithMany().HasForeignKey(e => e.CondominiumId).OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}