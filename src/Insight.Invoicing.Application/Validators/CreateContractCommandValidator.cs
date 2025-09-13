using FluentValidation;
using Insight.Invoicing.Application.Commands.Contracts;

namespace Insight.Invoicing.Application.Validators;

public class CreateContractCommandValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0)
            .WithMessage("Tenant ID must be greater than zero");

        RuleFor(x => x.ApartmentUnit)
            .NotEmpty()
            .WithMessage("Apartment unit is required")
            .MaximumLength(50)
            .WithMessage("Apartment unit cannot exceed 50 characters");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("Total amount must be greater than zero")
            .LessThanOrEqualTo(10_000_000)
            .WithMessage("Total amount cannot exceed 10,000,000");

        RuleFor(x => x.InitialPayment)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial payment cannot be negative");

        RuleFor(x => x.InitialPayment)
            .LessThan(x => x.TotalAmount)
            .WithMessage("Initial payment must be less than total amount");

        RuleFor(x => x.NumberOfInstallments)
            .GreaterThan(0)
            .WithMessage("Number of installments must be greater than zero")
            .LessThanOrEqualTo(120)
            .WithMessage("Number of installments cannot exceed 120");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(x => x.StartDate.AddYears(10))
            .WithMessage("Contract duration cannot exceed 10 years");
    }
}

