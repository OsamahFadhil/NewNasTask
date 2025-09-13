using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Services;
using Insight.Invoicing.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Infrastructure.Services;

public class InstallmentCalculatorService : IInstallmentCalculatorService
{
    private readonly ILogger<InstallmentCalculatorService> _logger;

    public InstallmentCalculatorService(ILogger<InstallmentCalculatorService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<Installment>> CalculateInstallmentScheduleAsync(
        Contract contract,
        int gracePeriodDays = 5,
        DateTime? firstInstallmentDate = null)
    {
        _logger.LogInformation("Calculating installment schedule for contract {ContractId}", contract.Id);

        var installments = new List<Installment>();
        var startDate = firstInstallmentDate ?? contract.StartDate;
        var totalAmount = contract.TotalAmount.Amount;
        var installmentCount = contract.NumberOfInstallments;
        var installmentAmount = ApplyBankersRounding(totalAmount / installmentCount);

        var totalCalculated = installmentAmount * installmentCount;
        var roundingDifference = totalAmount - totalCalculated;

        for (int i = 0; i < installmentCount; i++)
        {
            var installmentNumber = i + 1;
            var dueDate = startDate.AddMonths(i);
            var gracePeriodEndDate = dueDate.AddDays(gracePeriodDays);

            var currentAmount = installmentAmount;
            if (i == installmentCount - 1)
            {
                currentAmount += roundingDifference;
            }

            var installment = new Installment(
                contract.Id,
                dueDate,
                new Money(currentAmount, contract.TotalAmount.Currency),
                installmentNumber,
                gracePeriodDays);

            installments.Add(installment);
        }

        _logger.LogInformation(
            "Created {Count} installments for contract {ContractId} with total amount {TotalAmount}",
            installments.Count,
            contract.Id,
            totalAmount);

        return await Task.FromResult(installments);
    }

    public decimal CalculatePenalty(Installment installment, decimal penaltyRate, DateTime calculationDate)
    {
        if (penaltyRate < 0)
            throw new ArgumentException("Penalty rate cannot be negative", nameof(penaltyRate));

        if (installment.Status == Domain.Enums.InstallmentStatus.Paid)
            return 0; // No penalty for paid installments

        if (calculationDate.Date <= installment.GracePeriodEndDate)
            return 0; // No penalty during grace period

        var outstandingAmount = installment.Amount.Amount - installment.PaidAmount.Amount;
        if (outstandingAmount <= 0)
            return 0;

        var penalty = outstandingAmount * penaltyRate;
        return ApplyBankersRounding(penalty);
    }

    public decimal CalculateEarlyPaymentDiscount(Installment installment, DateTime paymentDate, decimal discountRate)
    {
        if (discountRate < 0)
            throw new ArgumentException("Discount rate cannot be negative", nameof(discountRate));

        if (paymentDate.Date >= installment.DueDate)
            return 0; // No discount for on-time or late payments

        if (installment.Status == Domain.Enums.InstallmentStatus.Paid)
            return 0; // No discount for already paid installments

        var discount = installment.Amount.Amount * discountRate;
        return ApplyBankersRounding(discount);
    }

    public decimal ApplyBankersRounding(decimal amount, int decimals = 2)
    {
        // Banker's rounding (round half to even)
        var multiplier = (decimal)Math.Pow(10, decimals);
        var rounded = Math.Round(amount * multiplier, MidpointRounding.ToEven);
        return rounded / multiplier;
    }
}
