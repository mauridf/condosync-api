using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;

namespace CondoSync.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.UnitId).IsRequired();
        builder.Property(e => e.ResidentId).IsRequired();
        builder.Property(e => e.TicketNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Description).IsRequired().HasColumnType("text");
        builder.Property(e => e.Category).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Subcategory).HasMaxLength(100);
        builder.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20).HasDefaultValue(TicketPriority.Normal);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue(TicketStatus.Open);
        builder.Property(e => e.SlaHours).HasDefaultValue(48);
        builder.Property(e => e.Resolution).HasColumnType("text");
        builder.Property(e => e.LocationType).HasMaxLength(50);
        builder.Property(e => e.LocationDescription).HasMaxLength(300);
        builder.Property(e => e.Rating).IsRequired(false);
        builder.Property(e => e.Feedback).HasColumnType("text");
        builder.Property(e => e.Cost).HasPrecision(10, 2);
        builder.Property(e => e.PaidBy).HasMaxLength(30);
        builder.Property(e => e.Attachments).HasColumnType("jsonb");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.CondominiumId, e.Status });
        builder.HasIndex(e => new { e.CondominiumId, e.Priority, e.Status });
        builder.HasIndex(e => new { e.AssignedTo, e.Status });
        builder.HasIndex(e => new { e.CondominiumId, e.TicketNumber });

        builder.HasOne<Unit>().WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Resident>().WithMany().HasForeignKey(e => e.ResidentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(e => e.AssignedTo).OnDelete(DeleteBehavior.SetNull);
    }
}