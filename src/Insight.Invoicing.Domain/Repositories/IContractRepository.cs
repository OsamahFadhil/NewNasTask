using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Enums;

namespace Insight.Invoicing.Domain.Repositories;

public interface IContractRepository : IRepository<Contract>
{
    Task<IEnumerable<Contract>> GetByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Contract>> GetByStatusAsync(ContractStatus status, CancellationToken cancellationToken = default);

    Task<IEnumerable<Contract>> GetPendingContractsAsync(CancellationToken cancellationToken = default);

    Task<Contract?> GetWithInstallmentsAsync(int contractId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Contract>> GetContractsWithOverdueInstallmentsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Contract>> GetByApartmentUnitAsync(string apartmentUnit, CancellationToken cancellationToken = default);

    Task<ContractStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

public record ContractStatistics(
    int TotalContracts,
    int PendingContracts,
    int ApprovedContracts,
    int RejectedContracts,
    int ClosedContracts,
    decimal TotalContractValue,
    decimal TotalPaidAmount,
    decimal TotalOutstandingAmount,
    int OverdueInstallments
);

