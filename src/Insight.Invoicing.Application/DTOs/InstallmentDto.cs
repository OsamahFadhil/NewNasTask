using Insight.Invoicing.Domain.Enums;

namespace Insight.Invoicing.Application.DTOs;

public record InstallmentDto(
    int Id,
    int ContractId,
    int SequenceNumber,
    DateTime DueDate,
    decimal Amount,
    decimal PaidAmount,
    decimal RemainingAmount,
    InstallmentStatus Status,
    decimal PenaltyAmount,
    decimal TotalAmountDue,
    DateTime GracePeriodEndDate,
    int OverdueDays,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record InstallmentSummaryDto(
    int Id,
    int SequenceNumber,
    DateTime DueDate,
    decimal Amount,
    decimal PaidAmount,
    InstallmentStatus Status,
    decimal PenaltyAmount,
    bool IsOverdue
);

public record ApplyPaymentDto(
    decimal Amount,
    DateTime PaymentDate
);

