using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Insight.Invoicing.Infrastructure.Persistence.Configurations;

public class PaymentReceiptConfiguration : IEntityTypeConfiguration<PaymentReceipt>
{
    public void Configure(EntityTypeBuilder<PaymentReceipt> builder)
    {
        builder.ToTable("PaymentReceipts");

        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Id)
            .ValueGeneratedOnAdd();

        builder.Property(pr => pr.ContractId)
            .IsRequired();

        builder.Property(pr => pr.InstallmentId)
            .IsRequired();

        builder.OwnsOne(pr => pr.AmountPaid, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("AmountPaid");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("AmountPaidCurrency");
        });

        builder.Property(pr => pr.PaymentDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(pr => pr.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pr => pr.Comments)
            .HasMaxLength(1000);

        builder.Property(pr => pr.ValidatedBy);

        builder.Property(pr => pr.ValidatedAt);

        builder.Property(pr => pr.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(pr => pr.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.OwnsOne(pr => pr.FileReference, fr =>
        {
            fr.Property(f => f.BucketName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("BucketName");

            fr.Property(f => f.ObjectName)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("ObjectName");

            fr.Property(f => f.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("OriginalFileName");

            fr.Property(f => f.FileSize)
                .IsRequired()
                .HasColumnName("FileSize");

            fr.Property(f => f.ContentType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("ContentType");
        });

        builder.HasOne(pr => pr.Contract)
            .WithMany()
            .HasForeignKey(pr => pr.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Installment)
            .WithMany()
            .HasForeignKey(pr => pr.InstallmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pr => pr.ContractId)
            .HasDatabaseName("IX_PaymentReceipts_ContractId");

        builder.HasIndex(pr => pr.InstallmentId)
            .HasDatabaseName("IX_PaymentReceipts_InstallmentId");

        builder.HasIndex(pr => pr.Status)
            .HasDatabaseName("IX_PaymentReceipts_Status");

        builder.HasIndex(pr => pr.PaymentDate)
            .HasDatabaseName("IX_PaymentReceipts_PaymentDate");

        builder.HasIndex(pr => pr.ValidatedBy)
            .HasDatabaseName("IX_PaymentReceipts_ValidatedBy");
    }
}

