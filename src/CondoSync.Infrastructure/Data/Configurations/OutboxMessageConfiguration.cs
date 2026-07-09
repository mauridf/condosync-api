using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.Type).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Content).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.Headers).HasColumnType("jsonb");
        builder.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
        builder.Property(e => e.RetryCount).HasDefaultValue(0);
        builder.Property(e => e.MaxRetries).HasDefaultValue(5);
        builder.Property(e => e.LastError).HasColumnType("text");
        builder.Property(e => e.ErrorStackTrace).HasColumnType("text");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.Status, e.CreatedAt }).HasFilter("status = 'pending'");
    }
}