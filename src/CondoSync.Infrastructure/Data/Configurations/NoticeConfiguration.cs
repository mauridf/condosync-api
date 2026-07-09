using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;

namespace CondoSync.Infrastructure.Data.Configurations;

public class NoticeConfiguration : IEntityTypeConfiguration<Notice>
{
    public void Configure(EntityTypeBuilder<Notice> builder)
    {
        builder.ToTable("notices");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.AuthorId).IsRequired();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Content).IsRequired().HasColumnType("text");
        builder.Property(e => e.Summary).HasMaxLength(500);
        builder.Property(e => e.Category).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Visibility).HasMaxLength(30).HasDefaultValue("all");
        builder.Property(e => e.TargetUnits).HasColumnType("jsonb");
        builder.Property(e => e.IsPinned).HasDefaultValue(false);
        builder.Property(e => e.IsUrgent).HasDefaultValue(false);
        builder.Property(e => e.ViewsCount).HasDefaultValue(0);
        builder.Property(e => e.UniqueViewsCount).HasDefaultValue(0);
        builder.Property(e => e.Attachments).HasColumnType("jsonb");
        builder.Property(e => e.Reactions).HasColumnType("jsonb");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.CondominiumId, e.PublishedAt }).HasFilter("published_at IS NOT NULL").IsDescending(false, true);
        builder.HasIndex(e => new { e.CondominiumId, e.Category });
        builder.HasIndex(e => new { e.IsPinned, e.PublishedAt }).HasFilter("is_pinned = true");

        builder.HasOne<User>().WithMany().HasForeignKey(e => e.AuthorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}