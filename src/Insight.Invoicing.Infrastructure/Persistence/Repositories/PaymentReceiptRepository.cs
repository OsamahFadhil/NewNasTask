using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Enums;
using Insight.Invoicing.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Insight.Invoicing.Infrastructure.Persistence.Repositories;

public class PaymentReceiptRepository : Repository<PaymentReceipt>, IPaymentReceiptRepository
{
    public PaymentReceiptRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PaymentReceipt>> GetByContractIdAsync(int contractId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pr => pr.Contract)
            .Include(pr => pr.Installment)
            .Where(pr => pr.ContractId == contractId)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentReceipt>> GetByInstallmentIdAsync(int installmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pr => pr.Contract)
            .Include(pr => pr.Installment)
            .Where(pr => pr.InstallmentId == installmentId)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentReceipt>> GetByStatusAsync(PaymentReceiptStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pr => pr.Contract)
            .ThenInclude(c => c.Tenant)
            .Include(pr => pr.Installment)
            .Where(pr => pr.Status == status)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentReceipt>> GetPendingValidationAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pr => pr.Contract)
            .ThenInclude(c => c.Tenant)
            .Include(pr => pr.Installment)
            .Where(pr => pr.Status == PaymentReceiptStatus.PendingValidation)
            .OrderBy(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentReceipt>> GetByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pr => pr.Contract)
            .Include(pr => pr.Installment)
            .Where(pr => pr.Contract.TenantId == tenantId)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentReceipt>> GetByValidatorIdAsync(int administratorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pr => pr.Contract)
            .ThenInclude(c => c.Tenant)
            .Include(pr => pr.Installment)
            .Where(pr => pr.ValidatedBy == administratorId)
            .OrderByDescending(pr => pr.ValidatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentReceipt>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pr => pr.Contract)
            .ThenInclude(c => c.Tenant)
            .Include(pr => pr.Installment)
            .Where(pr => pr.PaymentDate >= startDate.Date && pr.PaymentDate <= endDate.Date)
            .OrderByDescending(pr => pr.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentReceipt?> GetWithDetailsAsync(int receiptId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pr => pr.Contract)
            .ThenInclude(c => c.Tenant)
            .Include(pr => pr.Installment)
            .FirstOrDefaultAsync(pr => pr.Id == receiptId, cancellationToken);
    }

    public override async Task<PaymentReceipt?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(pr => pr.Contract)
            .Include(pr => pr.Installment)
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);
    }
}

