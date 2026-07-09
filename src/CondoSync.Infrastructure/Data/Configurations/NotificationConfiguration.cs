using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Body).HasColumnType("text");
        builder.Property(e => e.Type).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.EntityType).HasMaxLength(50);
        builder.Property(e => e.Action).HasMaxLength(50);
        builder.Property(e => e.Channels).HasColumnType("jsonb").HasDefaultValue("[\"in_app\"]");
        builder.Property(e => e.IsRead).HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt }).IsDescending(false, false, true);
        builder.HasIndex(e => new { e.CondominiumId, e.UserId }).HasFilter("is_read = false");

        builder.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}