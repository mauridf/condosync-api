using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.Action).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.OldValues).HasColumnType("jsonb");
        builder.Property(e => e.NewValues).HasColumnType("jsonb");
        builder.Property(e => e.Details).HasColumnType("jsonb");
        builder.Property(e => e.IpAddress).HasMaxLength(50);
        builder.Property(e => e.UserAgent).HasColumnType("text");
        builder.Property(e => e.UserRole).HasMaxLength(30);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.CondominiumId, e.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => new { e.UserId, e.CreatedAt }).IsDescending(false, true);
    }
}