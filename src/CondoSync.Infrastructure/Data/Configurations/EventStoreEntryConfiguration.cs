using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class EventStoreEntryConfiguration : IEntityTypeConfiguration<EventStoreEntry>
{
    public void Configure(EntityTypeBuilder<EventStoreEntry> builder)
    {
        builder.ToTable("event_store");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityColumn();
        builder.Property(e => e.AggregateType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.AggregateId).IsRequired();
        builder.Property(e => e.Version).IsRequired();
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(500);
        builder.Property(e => e.EventData).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.Metadata).HasColumnType("jsonb");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.AggregateType, e.AggregateId, e.Version }).IsUnique();
        builder.HasIndex(e => e.CreatedAt);
    }
}