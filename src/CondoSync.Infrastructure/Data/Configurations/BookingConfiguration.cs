using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;

namespace CondoSync.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.ServiceId).IsRequired();
        builder.Property(e => e.UnitId).IsRequired();
        builder.Property(e => e.ResidentId).IsRequired();
        builder.Property(e => e.BookingDate).IsRequired();
        builder.Property(e => e.StartTime).IsRequired();
        builder.Property(e => e.EndTime).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue(BookingStatus.Pending);
        builder.Property(e => e.Title).HasMaxLength(300);
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.GuestsCount).HasDefaultValue(0);
        builder.Property(e => e.SpecialRequirements).HasColumnType("text");
        builder.Property(e => e.RejectionReason).HasColumnType("text");
        builder.Property(e => e.CancellationReason).HasColumnType("text");
        builder.Property(e => e.CancelledBySystem).HasDefaultValue(false);
        builder.Property(e => e.Amount).HasPrecision(10, 2);
        builder.Property(e => e.PaymentStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.PaymentMethod).HasMaxLength(50);
        builder.Property(e => e.TransactionId).HasMaxLength(100);
        builder.Property(e => e.QrCodeUrl).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.BookingDate, e.ServiceId, e.Status });
        builder.HasIndex(e => e.ResidentId);
        builder.HasIndex(e => new { e.CondominiumId, e.Status });
        builder.HasIndex(e => e.PaymentStatus).HasFilter("amount > 0");

        builder.HasOne<Service>().WithMany().HasForeignKey(e => e.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Unit>().WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Resident>().WithMany().HasForeignKey(e => e.ResidentId).OnDelete(DeleteBehavior.Restrict);
    }
}