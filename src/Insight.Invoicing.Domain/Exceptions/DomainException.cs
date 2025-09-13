namespace Insight.Invoicing.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class InvalidContractStateException : DomainException
{
    public InvalidContractStateException(string message) : base(message)
    {
    }
}

public class InvalidInstallmentOperationException : DomainException
{
    public InvalidInstallmentOperationException(string message) : base(message)
    {
    }
}

public class InvalidPaymentAmountException : DomainException
{
    public InvalidPaymentAmountException(string message) : base(message)
    {
    }
}

public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }
}

