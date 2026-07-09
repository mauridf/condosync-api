using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;

namespace CondoSync.Infrastructure.Data.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("units");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.CondominiumId)
            .IsRequired();

        builder.Property(e => e.Block)
            .HasMaxLength(50);

        builder.Property(e => e.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Floor)
            .HasMaxLength(20);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(UnitType.Apartment);

        builder.Property(e => e.Area)
            .HasPrecision(10, 2);

        builder.Property(e => e.Bedrooms)
            .HasDefaultValue(0);

        builder.Property(e => e.Bathrooms)
            .HasDefaultValue(0);

        builder.Property(e => e.ParkingSpots)
            .HasDefaultValue(0);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.IsRented)
            .HasDefaultValue(false);

        builder.Property(e => e.OccupancyStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(UnitOccupancyStatus.Vacant);

        builder.Property(e => e.MonthlyFee)
            .HasPrecision(10, 2);

        builder.Property(e => e.LateFeePercentage)
            .HasPrecision(5, 2)
            .HasDefaultValue(2.00m);

        builder.Property(e => e.InterestPercentage)
            .HasPrecision(5, 2)
            .HasDefaultValue(0.033m);

        builder.Property(e => e.IptuAnnual)
            .HasPrecision(10, 2);

        builder.Property(e => e.CustomFields)
            .HasColumnType("jsonb");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Índices
        builder.HasIndex(e => e.CondominiumId);
        builder.HasIndex(e => new { e.CondominiumId, e.OccupancyStatus });

        // Unique constraint
        builder.HasIndex(e => new { e.CondominiumId, e.Block, e.Number })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");

        // Relacionamento
        builder.HasOne<Condominium>()
            .WithMany()
            .HasForeignKey(e => e.CondominiumId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete filter
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}