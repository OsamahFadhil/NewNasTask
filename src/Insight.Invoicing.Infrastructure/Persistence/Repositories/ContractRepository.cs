using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Enums;
using Insight.Invoicing.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Insight.Invoicing.Infrastructure.Persistence.Repositories;

public class ContractRepository : Repository<Contract>, IContractRepository
{
    public ContractRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Contract>> GetByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetByStatusAsync(ContractStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetPendingContractsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Status == ContractStatus.Submitted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Contract?> GetWithInstallmentsAsync(int contractId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Installments)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetContractsWithOverdueInstallmentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Installments)
            .Where(c => c.Installments.Any(i => i.IsOverdue()))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetByApartmentUnitAsync(string apartmentUnit, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ApartmentUnit == apartmentUnit)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ContractStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var contracts = await _dbSet
            .Include(c => c.Installments)
            .ToListAsync(cancellationToken);

        var totalContracts = contracts.Count;
        var pendingContracts = contracts.Count(c => c.Status == ContractStatus.Submitted);
        var approvedContracts = contracts.Count(c => c.Status == ContractStatus.Approved);
        var rejectedContracts = contracts.Count(c => c.Status == ContractStatus.NeedUpdate);
        var closedContracts = contracts.Count(c => c.Status == ContractStatus.Closed);

        var totalContractValue = contracts.Sum(c => c.TotalAmount.Amount);
        var totalPaidAmount = contracts.Sum(c => c.Installments.Sum(i => i.PaidAmount.Amount));
        var totalOutstandingAmount = totalContractValue - totalPaidAmount;

        var overdueInstallments = contracts.Sum(c => c.Installments.Count(i => i.IsOverdue()));

        return new ContractStatistics(
            totalContracts,
            pendingContracts,
            approvedContracts,
            rejectedContracts,
            closedContracts,
            totalContractValue,
            totalPaidAmount,
            totalOutstandingAmount,
            overdueInstallments
        );
    }
}
