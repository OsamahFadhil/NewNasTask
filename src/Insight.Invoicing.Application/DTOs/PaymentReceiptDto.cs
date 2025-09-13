using Insight.Invoicing.Domain.Enums;

namespace Insight.Invoicing.Application.DTOs;

public record PaymentReceiptDto(
    int Id,
    int ContractId,
    int InstallmentId,
    decimal AmountPaid,
    DateTime PaymentDate,
    PaymentReceiptStatus Status,
    string BucketName,
    string ObjectName,
    string OriginalFileName,
    long FileSize,
    string ContentType,
    string? Comments,
    int? ValidatedBy,
    string? ValidatedByName,
    DateTime? ValidatedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UploadPaymentReceiptDto(
    int ContractId,
    int InstallmentId,
    decimal AmountPaid,
    DateTime PaymentDate
);

public record ValidatePaymentReceiptDto(
    bool IsApproved,
    string? Comments
);

public record PaymentReceiptSummaryDto(
    int Id,
    decimal AmountPaid,
    DateTime PaymentDate,
    PaymentReceiptStatus Status,
    string FileName,
    string FileSize,
    DateTime CreatedAt
);

