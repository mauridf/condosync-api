using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;

namespace CondoSync.Infrastructure.Data.Configurations;

public class CondominiumConfiguration : IEntityTypeConfiguration<Condominium>
{
    public void Configure(EntityTypeBuilder<Condominium> builder)
    {
        builder.ToTable("condominiums");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Cnpj)
            .HasMaxLength(18);

        builder.HasIndex(e => e.Cnpj)
            .IsUnique()
            .HasFilter("cnpj IS NOT NULL");

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => e.Slug)
            .IsUnique();

        builder.Property(e => e.Address)
            .HasMaxLength(300);

        builder.Property(e => e.City)
            .HasMaxLength(100);

        builder.Property(e => e.State)
            .HasMaxLength(2);

        builder.Property(e => e.ZipCode)
            .HasMaxLength(9);

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.Email)
            .HasMaxLength(200);

        builder.Property(e => e.LogoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.SubscriptionPlan)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(SubscriptionPlan.Trial);

        builder.Property(e => e.SubscriptionStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(SubscriptionStatus.Trial);

        builder.Property(e => e.MaxUnits)
            .HasDefaultValue(0);

        builder.Property(e => e.MaxResidentsPerUnit)
            .HasDefaultValue(10);

        builder.Property(e => e.Timezone)
            .HasMaxLength(50)
            .HasDefaultValue("America/Sao_Paulo");

        builder.Property(e => e.Language)
            .HasMaxLength(10)
            .HasDefaultValue("pt-BR");

        builder.Property(e => e.EnabledModules)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[\"units\",\"residents\",\"notices\",\"tickets\"]'::jsonb");

        builder.Property(e => e.Settings)
            .HasColumnType("jsonb");

        builder.Property(e => e.Features)
            .HasColumnType("jsonb");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Índices
        builder.HasIndex(e => e.SubscriptionStatus);
        builder.HasIndex(e => e.TrialEndsAt)
            .HasFilter("subscription_plan = 'Trial'");

        // Soft delete filter
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}