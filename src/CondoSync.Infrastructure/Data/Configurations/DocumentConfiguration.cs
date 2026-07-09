using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.UploadedBy).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.DocumentType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.FileSize).IsRequired();
        builder.Property(e => e.Version).HasDefaultValue(1);
        builder.Property(e => e.Visibility).HasMaxLength(30).HasDefaultValue("all");
        builder.Property(e => e.IsActive).HasDefaultValue(true);
        builder.Property(e => e.RequiresSignature).HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.CondominiumId, e.DocumentType });
        builder.HasIndex(e => e.ExpiresAt).HasFilter("expires_at IS NOT NULL");

        builder.HasOne<User>().WithMany().HasForeignKey(e => e.UploadedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Document>().WithMany().HasForeignKey(e => e.PreviousVersionId).OnDelete(DeleteBehavior.SetNull);
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}