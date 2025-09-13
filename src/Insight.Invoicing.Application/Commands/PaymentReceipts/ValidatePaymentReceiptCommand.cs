using MediatR;

namespace Insight.Invoicing.Application.Commands.PaymentReceipts;

public record ValidatePaymentReceiptCommand(
    int PaymentReceiptId,
    bool IsApproved,
    int ValidatedBy,
    string? Comments = null
) : IRequest<bool>;

