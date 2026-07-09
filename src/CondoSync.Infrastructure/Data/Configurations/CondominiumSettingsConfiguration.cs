using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CondoSync.Core.Entities;

namespace CondoSync.Infrastructure.Data.Configurations;

public class CondominiumSettingsConfiguration : IEntityTypeConfiguration<CondominiumSettings>
{
    public void Configure(EntityTypeBuilder<CondominiumSettings> builder)
    {
        builder.ToTable("condominium_settings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CondominiumId).IsRequired();
        builder.Property(e => e.AllowSelfRegistration).HasDefaultValue(true);
        builder.Property(e => e.RequireAdminApproval).HasDefaultValue(true);
        builder.Property(e => e.AllowGuestRegistration).HasDefaultValue(true);
        builder.Property(e => e.MaxFamilyMembersPerUnit).HasDefaultValue(10);
        builder.Property(e => e.MaxPetsPerUnit).HasDefaultValue(3);
        builder.Property(e => e.InvoiceGenerationDay).HasDefaultValue(5);
        builder.Property(e => e.DueDay).HasDefaultValue(10);
        builder.Property(e => e.LateFeePercentage).HasPrecision(5, 2).HasDefaultValue(2.00m);
        builder.Property(e => e.LateInterestDaily).HasPrecision(5, 2).HasDefaultValue(0.033m);
        builder.Property(e => e.EarlyPaymentDiscountPercentage).HasPrecision(5, 2).HasDefaultValue(0);
        builder.Property(e => e.EarlyPaymentDays).HasDefaultValue(0);
        builder.Property(e => e.AutomaticBoletoGeneration).HasDefaultValue(false);
        builder.Property(e => e.EnablePix).HasDefaultValue(true);
        builder.Property(e => e.EnableCreditCard).HasDefaultValue(false);
        builder.Property(e => e.PaymentGateway).HasColumnType("jsonb");
        builder.Property(e => e.NotificationEmailTemplate).HasColumnType("jsonb");
        builder.Property(e => e.EmailFromName).HasMaxLength(200);
        builder.Property(e => e.EmailFromAddress).HasMaxLength(200);
        builder.Property(e => e.SmsEnabled).HasDefaultValue(false);
        builder.Property(e => e.SmsProvider).HasColumnType("jsonb");
        builder.Property(e => e.PrimaryColor).HasMaxLength(7).HasDefaultValue("#1976D2");
        builder.Property(e => e.SecondaryColor).HasMaxLength(7).HasDefaultValue("#FF9800");
        builder.Property(e => e.LogoUrl).HasMaxLength(500);
        builder.Property(e => e.FaviconUrl).HasMaxLength(500);
        builder.Property(e => e.CustomCss).HasColumnType("text");
        builder.Property(e => e.VisitorQrCodeEnabled).HasDefaultValue(true);
        builder.Property(e => e.VisitorNotifyOwner).HasDefaultValue(true);
        builder.Property(e => e.MaxVisitorsPerDay).HasDefaultValue(10);
        builder.Property(e => e.VisitorAutoApprove).HasDefaultValue(false);
        builder.Property(e => e.Integrations).HasColumnType("jsonb");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.CondominiumId).IsUnique();

        builder.HasOne<Condominium>().WithMany().HasForeignKey(e => e.CondominiumId).OnDelete(DeleteBehavior.Cascade);
    }
}