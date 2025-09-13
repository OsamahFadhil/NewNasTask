using Insight.Invoicing.Application.Commands.PaymentReceipts;
using Insight.Invoicing.Domain.Repositories;
using MediatR;

namespace Insight.Invoicing.Application.Handlers.PaymentReceipts;

public class ValidatePaymentReceiptCommandHandler : IRequestHandler<ValidatePaymentReceiptCommand, bool>
{
    private readonly IPaymentReceiptRepository _paymentReceiptRepository;

    public ValidatePaymentReceiptCommandHandler(IPaymentReceiptRepository paymentReceiptRepository)
    {
        _paymentReceiptRepository = paymentReceiptRepository;
    }

    public async Task<bool> Handle(ValidatePaymentReceiptCommand request, CancellationToken cancellationToken)
    {
        var paymentReceipt = await _paymentReceiptRepository.GetByIdAsync(request.PaymentReceiptId, cancellationToken);

        if (paymentReceipt == null)
        {
            return false;
        }

        if (request.IsApproved)
        {
            paymentReceipt.Approve(request.ValidatedBy, request.Comments);
        }
        else
        {
            paymentReceipt.Reject(request.ValidatedBy, request.Comments ?? "No reason provided");
        }

        await _paymentReceiptRepository.UpdateAsync(paymentReceipt, cancellationToken);

        return true;
    }
}
