using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;

namespace CondoSync.Infrastructure.Data.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("bills");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.UnitId).IsRequired();
        builder.Property(e => e.BillNumber).HasMaxLength(50);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(300);
        builder.Property(e => e.ReferenceMonth).IsRequired().HasMaxLength(7);
        builder.Property(e => e.BaseAmount).IsRequired().HasPrecision(10, 2);
        builder.Property(e => e.DiscountAmount).HasPrecision(10, 2).HasDefaultValue(0);
        builder.Property(e => e.DiscountType).HasMaxLength(30);
        builder.Property(e => e.FineAmount).HasPrecision(10, 2).HasDefaultValue(0);
        builder.Property(e => e.InterestAmount).HasPrecision(10, 2).HasDefaultValue(0);
        builder.Property(e => e.TotalAmount).IsRequired().HasPrecision(10, 2);
        builder.Property(e => e.Balance).HasPrecision(10, 2);
        builder.Property(e => e.IssueDate).IsRequired();
        builder.Property(e => e.DueDate).IsRequired();
        builder.Property(e => e.LateFeePercentage).HasPrecision(5, 2).HasDefaultValue(2.00m);
        builder.Property(e => e.LateInterestDaily).HasPrecision(5, 2).HasDefaultValue(0.033m);
        builder.Property(e => e.MaxInterestMonths).HasDefaultValue(12);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(e => e.PaymentAmount).HasPrecision(10, 2);
        builder.Property(e => e.PaymentMethod).HasMaxLength(50);
        builder.Property(e => e.TransactionId).HasMaxLength(100);
        builder.Property(e => e.InstallmentNumber).HasDefaultValue(1);
        builder.Property(e => e.TotalInstallments).HasDefaultValue(1);
        builder.Property(e => e.BoletoUrl).HasMaxLength(500);
        builder.Property(e => e.BoletoCode).HasMaxLength(100);
        builder.Property(e => e.PixCode).HasMaxLength(500);
        builder.Property(e => e.PixQrCodeUrl).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.UnitId, e.DueDate });
        builder.HasIndex(e => new { e.CondominiumId, e.Status });
        builder.HasIndex(e => new { e.CondominiumId, e.ReferenceMonth });
        builder.HasIndex(e => new { e.CondominiumId, e.Status }).HasFilter("status = 'Overdue'");

        builder.HasOne<Unit>().WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Bill>().WithMany().HasForeignKey(e => e.ParentBillId).OnDelete(DeleteBehavior.SetNull);
    }
}