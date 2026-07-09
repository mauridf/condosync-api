using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;

namespace CondoSync.Infrastructure.Data.Configurations;

public class VisitorConfiguration : IEntityTypeConfiguration<Visitor>
{
    public void Configure(EntityTypeBuilder<Visitor> builder)
    {
        builder.ToTable("visitors");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.UnitId).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Document).HasMaxLength(30);
        builder.Property(e => e.DocumentType).HasMaxLength(20);
        builder.Property(e => e.VehiclePlate).HasMaxLength(10);
        builder.Property(e => e.VehicleModel).HasMaxLength(100);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.VisitDate).IsRequired();
        builder.Property(e => e.VisitorType).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.CompanyName).HasMaxLength(200);
        builder.Property(e => e.ServiceDescription).HasMaxLength(300);
        builder.Property(e => e.AuthorizationCode).HasMaxLength(10);
        builder.Property(e => e.QrCodeUrl).HasMaxLength(500);
        builder.Property(e => e.IsRecurring).HasDefaultValue(false);
        builder.Property(e => e.RecurringSchedule).HasColumnType("jsonb");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue("Authorized");
        builder.Property(e => e.Notes).HasColumnType("text");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.CondominiumId, e.VisitDate });
        builder.HasIndex(e => e.AuthorizationCode).IsUnique().HasFilter("authorization_code IS NOT NULL");
        builder.HasIndex(e => new { e.UnitId, e.VisitDate });

        builder.HasOne<Unit>().WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Resident>().WithMany().HasForeignKey(e => e.ResidentId).OnDelete(DeleteBehavior.SetNull);
    }
}