using CondoSync.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoSync.Infrastructure.Data.Configurations;

public class GuestListConfiguration : IEntityTypeConfiguration<GuestList>
{
    public void Configure(EntityTypeBuilder<GuestList> builder)
    {
        builder.ToTable("guest_lists");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.EventDate).IsRequired();
        builder.Property(x => x.MaxGuests).HasDefaultValue(50);
        builder.Property(x => x.RequiresQrCode).HasDefaultValue(true);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(x => x.CondominiumId);
        builder.HasIndex(x => x.EventDate);
        builder.HasIndex(x => x.BookingId);
    }
}
