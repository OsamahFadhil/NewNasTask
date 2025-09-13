using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Enums;

namespace Insight.Invoicing.Domain.Repositories;

public interface IPaymentReceiptRepository : IRepository<PaymentReceipt>
{
    Task<IEnumerable<PaymentReceipt>> GetByContractIdAsync(int contractId, CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentReceipt>> GetByInstallmentIdAsync(int installmentId, CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentReceipt>> GetByStatusAsync(PaymentReceiptStatus status, CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentReceipt>> GetPendingValidationAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentReceipt>> GetByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentReceipt>> GetByValidatorIdAsync(int administratorId, CancellationToken cancellationToken = default);

    Task<IEnumerable<PaymentReceipt>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<PaymentReceipt?> GetWithDetailsAsync(int receiptId, CancellationToken cancellationToken = default);
}

