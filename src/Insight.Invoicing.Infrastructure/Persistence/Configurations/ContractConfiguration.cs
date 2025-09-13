using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Enums;
using Insight.Invoicing.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Insight.Invoicing.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.TenantId)
            .IsRequired();

        builder.Property(c => c.ApartmentUnit)
            .IsRequired()
            .HasMaxLength(50);

        builder.OwnsOne(c => c.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("TotalAmount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("TotalAmountCurrency");
        });

        builder.OwnsOne(c => c.InitialPayment, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("InitialPayment");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("InitialPaymentCurrency");
        });

        builder.Property(c => c.NumberOfInstallments)
            .IsRequired();

        builder.Property(c => c.StartDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(c => c.EndDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.Comments)
            .HasMaxLength(1000);

        builder.Property(c => c.LastUpdatedBy);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Installments)
            .WithOne(i => i.Contract)
            .HasForeignKey(i => i.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("IX_Contracts_TenantId");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Contracts_Status");

        builder.HasIndex(c => c.ApartmentUnit)
            .HasDatabaseName("IX_Contracts_ApartmentUnit");

        builder.HasIndex(c => new { c.StartDate, c.EndDate })
            .HasDatabaseName("IX_Contracts_DateRange");
    }
}

