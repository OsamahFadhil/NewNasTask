using Insight.Invoicing.Domain.Enums;
using Insight.Invoicing.Domain.Exceptions;
using Insight.Invoicing.Domain.ValueObjects;
using Insight.Invoicing.Shared.Common;

namespace Insight.Invoicing.Domain.Entities;

public class Installment : BaseEntity
{
    public int ContractId { get; private set; }

    public DateTime DueDate { get; private set; }

    public Money Amount { get; private set; } = null!;

    public Money PaidAmount { get; private set; } = null!;

    public InstallmentStatus Status { get; private set; } = InstallmentStatus.Pending;

    public Money PenaltyAmount { get; private set; } = null!;

    public DateTime GracePeriodEndDate { get; private set; }

    public int SequenceNumber { get; private set; }

    public Contract Contract { get; private set; } = null!;

    private Installment() { }

    public Installment(int contractId, DateTime dueDate, Money amount, int gracePeriodDays, int sequenceNumber)
    {
        if (contractId <= 0)
            throw new ArgumentException("Contract ID must be greater than zero", nameof(contractId));

        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        if (sequenceNumber <= 0)
            throw new ArgumentException("Sequence number must be greater than zero", nameof(sequenceNumber));

        ContractId = contractId;
        DueDate = dueDate.Date;
        Amount = amount;
        PaidAmount = Money.Zero(amount.Currency);
        PenaltyAmount = Money.Zero(amount.Currency);
        GracePeriodEndDate = dueDate.Date.AddDays(gracePeriodDays);
        SequenceNumber = sequenceNumber;
    }

    public Money GetRemainingAmount() => Amount - PaidAmount;

    public Money GetTotalAmountDue() => Amount + PenaltyAmount - PaidAmount;

    public bool IsOverdue() => DateTime.UtcNow.Date > GracePeriodEndDate && Status != InstallmentStatus.Paid;

    public void ApplyPayment(Money paymentAmount, DateTime paymentDate)
    {
        if (paymentAmount == null)
            throw new ArgumentNullException(nameof(paymentAmount));

        if (paymentAmount.Amount <= 0)
            throw new InvalidPaymentAmountException("Payment amount must be greater than zero");

        if (paymentAmount.Currency != Amount.Currency)
            throw new InvalidPaymentAmountException("Payment currency must match installment currency");

        if (Status == InstallmentStatus.Paid)
            throw new InvalidInstallmentOperationException("Cannot apply payment to a fully paid installment");

        var totalAmountDue = GetTotalAmountDue();
        if (paymentAmount.IsGreaterThan(totalAmountDue))
            throw new InvalidPaymentAmountException($"Payment amount ({paymentAmount}) cannot exceed total amount due ({totalAmountDue})");

        PaidAmount += paymentAmount;

        var totalDue = Amount + PenaltyAmount;
        if (PaidAmount.IsGreaterThan(totalDue) || PaidAmount.Amount == totalDue.Amount)
        {
            Status = InstallmentStatus.Paid;
        }
        else if (PaidAmount.Amount > 0)
        {
            Status = InstallmentStatus.PartiallyPaid;
        }

        UpdateTimestamp();
    }

    public void CalculatePenalty(decimal penaltyRate, DateTime calculationDate)
    {
        if (penaltyRate < 0)
            throw new ArgumentException("Penalty rate cannot be negative", nameof(penaltyRate));

        if (Status == InstallmentStatus.Paid)
            return; // No penalty for paid installments

        if (calculationDate.Date <= GracePeriodEndDate)
            return; // No penalty during grace period

        var outstandingAmount = Amount - PaidAmount;
        if (outstandingAmount.Amount <= 0)
            return;

        var newPenalty = outstandingAmount * penaltyRate;

        if (newPenalty.IsGreaterThan(PenaltyAmount))
        {
            PenaltyAmount = newPenalty;
            Status = InstallmentStatus.Overdue;
            UpdateTimestamp();
        }
    }

    public int GetOverdueDays()
    {
        if (!IsOverdue())
            return 0;

        return (DateTime.UtcNow.Date - GracePeriodEndDate).Days;
    }

    public bool CanPayEarly(DateTime paymentDate)
    {
        return paymentDate.Date < DueDate && Status != InstallmentStatus.Paid;
    }

    public void MarkAsOverdue()
    {
        if (Status == InstallmentStatus.Pending)
        {
            Status = InstallmentStatus.Overdue;
            UpdateTimestamp();
        }
    }
}
