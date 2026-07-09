using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class PollVoteConfiguration : IEntityTypeConfiguration<PollVote>
{
    public void Configure(EntityTypeBuilder<PollVote> builder)
    {
        builder.ToTable("poll_votes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.PollId).IsRequired();
        builder.Property(e => e.SelectedOptions).IsRequired().HasColumnType("uuid[]");
        builder.Property(e => e.VotedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.PollId);

        builder.HasOne<Poll>().WithMany().HasForeignKey(e => e.PollId).OnDelete(DeleteBehavior.Cascade);
    }
}