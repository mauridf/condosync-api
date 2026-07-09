using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class UnitInvitationConfiguration : IEntityTypeConfiguration<UnitInvitation>
{
    public void Configure(EntityTypeBuilder<UnitInvitation> builder)
    {
        builder.ToTable("unit_invitations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.UnitId).IsRequired();
        builder.Property(e => e.InvitationCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.InvitationUrl).HasMaxLength(500);
        builder.Property(e => e.MaxUses).HasDefaultValue(1);
        builder.Property(e => e.UsesCount).HasDefaultValue(0);
        builder.Property(e => e.RecipientEmail).HasMaxLength(200);
        builder.Property(e => e.RecipientName).HasMaxLength(200);
        builder.Property(e => e.RecipientPhone).HasMaxLength(20);
        builder.Property(e => e.AccessType).HasMaxLength(30).HasDefaultValue("owner");
        builder.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("active");
        builder.Property(e => e.CreatedBy).IsRequired();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.InvitationCode).IsUnique();
        builder.HasIndex(e => e.UnitId);

        builder.HasOne<Unit>().WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    }
}