using Insight.Invoicing.Domain.Entities;

namespace Insight.Invoicing.Domain.Services;

public interface IInstallmentCalculatorService
{
    Task<IEnumerable<Installment>> CalculateInstallmentScheduleAsync(
        Contract contract,
        int gracePeriodDays = 5,
        DateTime? firstInstallmentDate = null);

    decimal CalculatePenalty(Installment installment, decimal penaltyRate, DateTime calculationDate);

    decimal CalculateEarlyPaymentDiscount(Installment installment, DateTime paymentDate, decimal discountRate);

    decimal ApplyBankersRounding(decimal amount, int decimals = 2);
}

