using System.Linq.Expressions;
using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Enums;

namespace Insight.Invoicing.Application.Specifications;

public class OverdueInstallmentsSpec : Specification<Installment>
{
    private readonly DateTime _currentDate;

    public OverdueInstallmentsSpec(DateTime? currentDate = null)
    {
        _currentDate = currentDate ?? DateTime.UtcNow;
    }

    public override Expression<Func<Installment, bool>> ToExpression()
    {
        return installment => installment.GracePeriodEndDate < _currentDate
                            && installment.Status != InstallmentStatus.Paid;
    }
}

public class InstallmentsByContractSpec : Specification<Installment>
{
    private readonly int _contractId;

    public InstallmentsByContractSpec(int contractId)
    {
        _contractId = contractId;
    }

    public override Expression<Func<Installment, bool>> ToExpression()
    {
        return installment => installment.ContractId == _contractId;
    }
}

public class InstallmentsByStatusSpec : Specification<Installment>
{
    private readonly InstallmentStatus _status;

    public InstallmentsByStatusSpec(InstallmentStatus status)
    {
        _status = status;
    }

    public override Expression<Func<Installment, bool>> ToExpression()
    {
        return installment => installment.Status == _status;
    }
}

public class InstallmentsDueInRangeSpec : Specification<Installment>
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public InstallmentsDueInRangeSpec(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
    }

    public override Expression<Func<Installment, bool>> ToExpression()
    {
        return installment => installment.DueDate >= _startDate && installment.DueDate <= _endDate;
    }
}

public class UnpaidInstallmentsSpec : Specification<Installment>
{
    public override Expression<Func<Installment, bool>> ToExpression()
    {
        return installment => installment.Status != InstallmentStatus.Paid;
    }
}

public class InstallmentsWithPenaltiesSpec : Specification<Installment>
{
    public override Expression<Func<Installment, bool>> ToExpression()
    {
        return installment => installment.PenaltyAmount.Amount > 0;
    }
}

public class InstallmentsOverdueByDaysSpec : Specification<Installment>
{
    private readonly int _minOverdueDays;
    private readonly DateTime _currentDate;

    public InstallmentsOverdueByDaysSpec(int minOverdueDays, DateTime? currentDate = null)
    {
        _minOverdueDays = minOverdueDays;
        _currentDate = currentDate ?? DateTime.UtcNow;
    }

    public override Expression<Func<Installment, bool>> ToExpression()
    {
        var cutoffDate = _currentDate.AddDays(-_minOverdueDays);
        return installment => installment.GracePeriodEndDate <= cutoffDate
                            && installment.Status != InstallmentStatus.Paid;
    }
}

