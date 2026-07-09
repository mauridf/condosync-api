using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;

namespace CondoSync.Infrastructure.Data.Configurations;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.ToTable("polls");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.PollType).HasConversion<string>().HasMaxLength(30).HasDefaultValue("Single");
        builder.Property(e => e.IsAnonymous).HasDefaultValue(false);
        builder.Property(e => e.IsMandatory).HasDefaultValue(false);
        builder.Property(e => e.RequiresUnitVote).HasDefaultValue(false);
        builder.Property(e => e.Options).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.StartsAt).IsRequired();
        builder.Property(e => e.EndsAt).IsRequired();
        builder.Property(e => e.TotalVotes).HasDefaultValue(0);
        builder.Property(e => e.ResultsVisibility).HasMaxLength(30).HasDefaultValue("after_end");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue("Draft");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.CondominiumId, e.Status });

        builder.HasOne<User>().WithMany().HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}