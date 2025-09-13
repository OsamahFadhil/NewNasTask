using Insight.Invoicing.Domain.Enums;
using Insight.Invoicing.Domain.Events;
using Insight.Invoicing.Domain.Exceptions;
using Insight.Invoicing.Domain.ValueObjects;
using Insight.Invoicing.Shared.Common;

namespace Insight.Invoicing.Domain.Entities;

public class Contract : AggregateRoot
{
    public int TenantId { get; private set; }

    public string ApartmentUnit { get; private set; } = string.Empty;

    public Money TotalAmount { get; private set; } = null!;

    public Money InitialPayment { get; private set; } = null!;

    public int NumberOfInstallments { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public ContractStatus Status { get; private set; } = ContractStatus.Draft;

    public string? Comments { get; private set; }

    public int? LastUpdatedBy { get; private set; }

    private readonly List<Installment> _installments = new();

    public IReadOnlyCollection<Installment> Installments => _installments.AsReadOnly();

    public User Tenant { get; private set; } = null!;

    private Contract() { }

    public Contract(
        int tenantId,
        string apartmentUnit,
        Money totalAmount,
        Money initialPayment,
        int numberOfInstallments,
        DateTime startDate,
        DateTime endDate)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(apartmentUnit))
            throw new ArgumentException("Apartment unit cannot be null or empty", nameof(apartmentUnit));

        if (totalAmount == null)
            throw new ArgumentNullException(nameof(totalAmount));

        if (initialPayment == null)
            throw new ArgumentNullException(nameof(initialPayment));

        if (totalAmount.Amount <= 0)
            throw new ArgumentException("Total amount must be greater than zero", nameof(totalAmount));

        if (initialPayment.Amount < 0)
            throw new ArgumentException("Initial payment cannot be negative", nameof(initialPayment));

        if (initialPayment.Currency != totalAmount.Currency)
            throw new ArgumentException("Initial payment and total amount must have the same currency", nameof(initialPayment));

        if (initialPayment.IsGreaterThan(totalAmount))
            throw new ArgumentException("Initial payment cannot be greater than total amount", nameof(initialPayment));

        if (numberOfInstallments <= 0)
            throw new ArgumentException("Number of installments must be greater than zero", nameof(numberOfInstallments));

        if (startDate > endDate)
            throw new ArgumentException("Start date must be before or equal to end date", nameof(startDate));

        TenantId = tenantId;
        ApartmentUnit = apartmentUnit.Trim();
        TotalAmount = totalAmount;
        InitialPayment = initialPayment;
        NumberOfInstallments = numberOfInstallments;
        StartDate = startDate.Date;
        EndDate = endDate.Date;
    }

    public Money GetRemainingAmount() => TotalAmount - InitialPayment;

    public Money GetInstallmentAmount() => GetRemainingAmount() * (1m / NumberOfInstallments);

    public void Submit()
    {
        if (Status != ContractStatus.Draft && Status != ContractStatus.NeedUpdate)
            throw new InvalidContractStateException($"Contract cannot be submitted from {Status} status");

        Status = ContractStatus.Submitted;
        UpdateTimestamp();

        AddDomainEvent(new ContractSubmittedEvent(Id, TenantId, TotalAmount.Amount));
    }

    public void Approve(int approvedBy, string? comments = null)
    {
        if (Status != ContractStatus.Submitted)
            throw new InvalidContractStateException($"Contract cannot be approved from {Status} status");

        Status = ContractStatus.Approved;
        LastUpdatedBy = approvedBy;
        Comments = comments;
        UpdateTimestamp();

        AddDomainEvent(new ContractApprovedEvent(Id, TenantId, approvedBy, TotalAmount.Amount, NumberOfInstallments));
    }

    public void Reject(int rejectedBy, string reason)
    {
        if (Status != ContractStatus.Submitted)
            throw new InvalidContractStateException($"Contract cannot be rejected from {Status} status");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason cannot be null or empty", nameof(reason));

        Status = ContractStatus.Draft;
        LastUpdatedBy = rejectedBy;
        Comments = reason;
        UpdateTimestamp();

        AddDomainEvent(new ContractRejectedEvent(Id, TenantId, rejectedBy, reason));
    }

    public void RequestUpdate(int requestedBy, string reason)
    {
        if (Status != ContractStatus.Submitted)
            throw new InvalidContractStateException($"Update cannot be requested from {Status} status");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Update reason cannot be null or empty", nameof(reason));

        Status = ContractStatus.NeedUpdate;
        LastUpdatedBy = requestedBy;
        Comments = reason;
        UpdateTimestamp();

        AddDomainEvent(new ContractUpdateRequestedEvent(Id, TenantId, requestedBy, reason));
    }

    public void Suspend(int suspendedBy, string reason)
    {
        if (Status != ContractStatus.Approved)
            throw new InvalidContractStateException($"Contract cannot be suspended from {Status} status");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Suspension reason cannot be null or empty", nameof(reason));

        Status = ContractStatus.Suspended;
        LastUpdatedBy = suspendedBy;
        Comments = reason;
        UpdateTimestamp();
    }

    public void Close(int closedBy, string reason)
    {
        if (Status != ContractStatus.Approved && Status != ContractStatus.Suspended)
            throw new InvalidContractStateException($"Contract cannot be closed from {Status} status");

        Status = ContractStatus.Closed;
        LastUpdatedBy = closedBy;
        Comments = reason;
        UpdateTimestamp();
    }

    public void UpdateDetails(
        string apartmentUnit,
        Money totalAmount,
        Money initialPayment,
        int numberOfInstallments,
        DateTime startDate,
        DateTime endDate)
    {
        if (Status != ContractStatus.Draft && Status != ContractStatus.NeedUpdate)
            throw new InvalidContractStateException($"Contract details cannot be updated from {Status} status");

        // Validate parameters (same validation as constructor)
        if (string.IsNullOrWhiteSpace(apartmentUnit))
            throw new ArgumentException("Apartment unit cannot be null or empty", nameof(apartmentUnit));

        if (totalAmount == null)
            throw new ArgumentNullException(nameof(totalAmount));

        if (initialPayment == null)
            throw new ArgumentNullException(nameof(initialPayment));

        if (totalAmount.Amount <= 0)
            throw new ArgumentException("Total amount must be greater than zero", nameof(totalAmount));

        if (initialPayment.Amount < 0)
            throw new ArgumentException("Initial payment cannot be negative", nameof(initialPayment));

        if (initialPayment.Currency != totalAmount.Currency)
            throw new ArgumentException("Initial payment and total amount must have the same currency", nameof(initialPayment));

        if (initialPayment.IsGreaterThan(totalAmount))
            throw new ArgumentException("Initial payment cannot be greater than total amount", nameof(initialPayment));

        if (numberOfInstallments <= 0)
            throw new ArgumentException("Number of installments must be greater than zero", nameof(numberOfInstallments));

        if (startDate > endDate)
            throw new ArgumentException("Start date must be before or equal to end date", nameof(startDate));

        ApartmentUnit = apartmentUnit.Trim();
        TotalAmount = totalAmount;
        InitialPayment = initialPayment;
        NumberOfInstallments = numberOfInstallments;
        StartDate = startDate.Date;
        EndDate = endDate.Date;

        UpdateTimestamp();
    }

    public void AddInstallments(IEnumerable<Installment> installments)
    {
        if (Status != ContractStatus.Approved)
            throw new InvalidContractStateException("Installments can only be added to approved contracts");

        if (!_installments.Any())
        {
            _installments.AddRange(installments);
            UpdateTimestamp();
        }
    }

    public Money GetTotalPaidAmount()
    {
        var totalPaidFromInstallments = _installments.Aggregate(
            Money.Zero(InitialPayment.Currency),
            (sum, installment) => sum + installment.PaidAmount);
        return InitialPayment + totalPaidFromInstallments;
    }

    public Money GetTotalOutstandingAmount()
    {
        return _installments.Aggregate(
            Money.Zero(TotalAmount.Currency),
            (sum, installment) => sum + installment.GetTotalAmountDue());
    }

    public bool IsFullyPaid()
    {
        return _installments.All(i => i.Status == InstallmentStatus.Paid);
    }

    public IEnumerable<Installment> GetOverdueInstallments()
    {
        return _installments.Where(i => i.IsOverdue());
    }
}
