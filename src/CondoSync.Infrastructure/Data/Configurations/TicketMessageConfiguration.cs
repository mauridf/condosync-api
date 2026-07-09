using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class TicketMessageConfiguration : IEntityTypeConfiguration<TicketMessage>
{
    public void Configure(EntityTypeBuilder<TicketMessage> builder)
    {
        builder.ToTable("ticket_messages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.TicketId).IsRequired();
        builder.Property(e => e.SenderId).IsRequired();
        builder.Property(e => e.Message).IsRequired().HasColumnType("text");
        builder.Property(e => e.IsInternal).HasDefaultValue(false);
        builder.Property(e => e.IsSystemMessage).HasDefaultValue(false);
        builder.Property(e => e.Attachments).HasColumnType("jsonb");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.TicketId);

        builder.HasOne<Ticket>().WithMany().HasForeignKey(e => e.TicketId).OnDelete(DeleteBehavior.Cascade);
    }
}