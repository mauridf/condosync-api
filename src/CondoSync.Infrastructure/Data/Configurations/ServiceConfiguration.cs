using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;

namespace CondoSync.Infrastructure.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Slug).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.Icon).HasMaxLength(100);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ServiceType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.RequiresApproval).HasDefaultValue(false);
        builder.Property(e => e.RequiresPayment).HasDefaultValue(false);
        builder.Property(e => e.MaxBookingPerDay).IsRequired(false);
        builder.Property(e => e.MaxBookingPerUser).IsRequired(false);
        builder.Property(e => e.AdvanceBookingDays).HasDefaultValue(0);
        builder.Property(e => e.CancelBeforeHours).HasDefaultValue(24);
        builder.Property(e => e.AllowSimultaneous).HasDefaultValue(false);
        builder.Property(e => e.AvailableDays).HasColumnType("jsonb");
        builder.Property(e => e.AllowCustomTime).HasDefaultValue(false);
        builder.Property(e => e.BlockedDates).HasColumnType("jsonb");
        builder.Property(e => e.Price).HasPrecision(10, 2).HasDefaultValue(0);
        builder.Property(e => e.PriceUnit).HasMaxLength(20);
        builder.Property(e => e.Rules).HasColumnType("jsonb");
        builder.Property(e => e.TermsOfUse).HasColumnType("text");
        builder.Property(e => e.IsActive).HasDefaultValue(true);
        builder.Property(e => e.DisplayOrder).HasDefaultValue(0);
        builder.Property(e => e.Images).HasColumnType("jsonb");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.CondominiumId, e.Slug }).IsUnique().HasFilter("deleted_at IS NULL");
        builder.HasIndex(e => new { e.CondominiumId, e.IsActive });

        builder.HasOne<Condominium>().WithMany().HasForeignKey(e => e.CondominiumId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}