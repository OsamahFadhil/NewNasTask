using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Application.Queries.Installments;
using Insight.Invoicing.Domain.Repositories;
using MediatR;

namespace Insight.Invoicing.Application.Handlers.Installments;

public class GetContractInstallmentsQueryHandler : IRequestHandler<GetContractInstallmentsQuery, (IEnumerable<InstallmentDto> Installments, int TotalCount)>
{
    private readonly IContractRepository _contractRepository;

    public GetContractInstallmentsQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<(IEnumerable<InstallmentDto> Installments, int TotalCount)> Handle(GetContractInstallmentsQuery request, CancellationToken cancellationToken)
    {
        var contract = await _contractRepository.GetWithInstallmentsAsync(request.ContractId, cancellationToken);

        if (contract == null)
        {
            return (Enumerable.Empty<InstallmentDto>(), 0);
        }

        var installments = contract.Installments
            .OrderBy(i => i.SequenceNumber)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new InstallmentDto(
                i.Id,
                i.ContractId,
                i.SequenceNumber,
                i.DueDate,
                i.Amount.Amount,
                i.PaidAmount.Amount,
                i.Amount.Amount - i.PaidAmount.Amount, // RemainingAmount
                i.Status,
                i.PenaltyAmount.Amount,
                i.Amount.Amount + i.PenaltyAmount.Amount, // TotalAmountDue
                i.GracePeriodEndDate,
                Math.Max(0, (DateTime.UtcNow - i.DueDate).Days), // OverdueDays
                i.CreatedAt,
                i.UpdatedAt));

        var totalCount = contract.Installments.Count();

        return (installments, totalCount);
    }
}
