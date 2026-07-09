using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class CommonAreaConfiguration : IEntityTypeConfiguration<CommonArea>
{
    public void Configure(EntityTypeBuilder<CommonArea> builder)
    {
        builder.ToTable("common_areas");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.Type).IsRequired().HasMaxLength(50);
        builder.Property(e => e.MaxGuestsPerResident).HasDefaultValue(5);
        builder.Property(e => e.Rules).HasColumnType("jsonb");
        builder.Property(e => e.RequiresBooking).HasDefaultValue(false);
        builder.Property(e => e.RequiresDeposit).HasDefaultValue(false);
        builder.Property(e => e.DepositAmount).HasPrecision(10, 2);
        builder.Property(e => e.OperatingHours).HasColumnType("jsonb");
        builder.Property(e => e.IsActive).HasDefaultValue(true);
        builder.Property(e => e.MaintenanceStatus).HasMaxLength(30).HasDefaultValue("operational");
        builder.Property(e => e.ScheduledMaintenance).HasColumnType("jsonb");
        builder.Property(e => e.Images).HasColumnType("jsonb");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.CondominiumId, e.IsActive });

        builder.HasOne<Condominium>().WithMany().HasForeignKey(e => e.CondominiumId).OnDelete(DeleteBehavior.Restrict);
    }
}