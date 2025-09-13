using System.Linq.Expressions;
using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Enums;

namespace Insight.Invoicing.Application.Specifications;

public class ContractWithInstallmentsSpec : Specification<Contract>
{
    public override Expression<Func<Contract, bool>> ToExpression()
    {
        // This will be handled in the repository implementation with Include()
        return contract => true;
    }
}

public class ContractByTenantSpec : Specification<Contract>
{
    private readonly int _tenantId;

    public ContractByTenantSpec(int tenantId)
    {
        _tenantId = tenantId;
    }

    public override Expression<Func<Contract, bool>> ToExpression()
    {
        return contract => contract.TenantId == _tenantId;
    }
}

public class ContractByStatusSpec : Specification<Contract>
{
    private readonly ContractStatus _status;

    public ContractByStatusSpec(ContractStatus status)
    {
        _status = status;
    }

    public override Expression<Func<Contract, bool>> ToExpression()
    {
        return contract => contract.Status == _status;
    }
}

public class PendingContractsSpec : Specification<Contract>
{
    public override Expression<Func<Contract, bool>> ToExpression()
    {
        return contract => contract.Status == ContractStatus.Submitted;
    }
}

public class ActiveContractsSpec : Specification<Contract>
{
    public override Expression<Func<Contract, bool>> ToExpression()
    {
        return contract => contract.Status == ContractStatus.Approved;
    }
}

public class ContractByApartmentUnitSpec : Specification<Contract>
{
    private readonly string _apartmentUnit;

    public ContractByApartmentUnitSpec(string apartmentUnit)
    {
        _apartmentUnit = apartmentUnit;
    }

    public override Expression<Func<Contract, bool>> ToExpression()
    {
        return contract => contract.ApartmentUnit == _apartmentUnit;
    }
}

public class ContractByDateRangeSpec : Specification<Contract>
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public ContractByDateRangeSpec(DateTime startDate, DateTime endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
    }

    public override Expression<Func<Contract, bool>> ToExpression()
    {
        return contract => contract.StartDate >= _startDate && contract.StartDate <= _endDate;
    }
}

