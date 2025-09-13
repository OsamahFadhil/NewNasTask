using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Insight.Invoicing.Infrastructure.Persistence.Configurations;

public class InstallmentConfiguration : IEntityTypeConfiguration<Installment>
{
    public void Configure(EntityTypeBuilder<Installment> builder)
    {
        builder.ToTable("Installments");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.ContractId)
            .IsRequired();

        builder.Property(i => i.DueDate)
            .IsRequired()
            .HasColumnType("date");

        builder.OwnsOne(i => i.Amount, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("Amount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("AmountCurrency");
        });

        builder.OwnsOne(i => i.PaidAmount, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("PaidAmount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("PaidAmountCurrency");
        });

        builder.OwnsOne(i => i.PenaltyAmount, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("PenaltyAmount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("PenaltyAmountCurrency");
        });

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.GracePeriodEndDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(i => i.SequenceNumber)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(i => i.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(i => i.Contract)
            .WithMany(c => c.Installments)
            .HasForeignKey(i => i.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.ContractId)
            .HasDatabaseName("IX_Installments_ContractId");

        builder.HasIndex(i => i.DueDate)
            .HasDatabaseName("IX_Installments_DueDate");

        builder.HasIndex(i => i.Status)
            .HasDatabaseName("IX_Installments_Status");

        builder.HasIndex(i => i.GracePeriodEndDate)
            .HasDatabaseName("IX_Installments_GracePeriodEndDate");

        builder.HasIndex(i => new { i.ContractId, i.SequenceNumber })
            .IsUnique()
            .HasDatabaseName("IX_Installments_Contract_Sequence");
    }
}

