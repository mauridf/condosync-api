using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class NoticeCommentConfiguration : IEntityTypeConfiguration<NoticeComment>
{
    public void Configure(EntityTypeBuilder<NoticeComment> builder)
    {
        builder.ToTable("notice_comments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.NoticeId).IsRequired();
        builder.Property(e => e.AuthorId).IsRequired();
        builder.Property(e => e.Content).IsRequired().HasColumnType("text");
        builder.Property(e => e.IsEdited).HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.NoticeId);

        builder.HasOne<Notice>().WithMany().HasForeignKey(e => e.NoticeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>().WithMany().HasForeignKey(e => e.AuthorId).OnDelete(DeleteBehavior.Restrict);
    }
}