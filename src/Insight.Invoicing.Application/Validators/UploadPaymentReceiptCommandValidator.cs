using FluentValidation;
using Insight.Invoicing.Application.Commands.PaymentReceipts;

namespace Insight.Invoicing.Application.Validators;

public class UploadPaymentReceiptCommandValidator : AbstractValidator<UploadPaymentReceiptCommand>
{
    private static readonly string[] AllowedContentTypes =
    {
        "application/pdf",
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public UploadPaymentReceiptCommandValidator()
    {
        RuleFor(x => x.ContractId)
            .GreaterThan(0)
            .WithMessage("Contract ID must be greater than zero");

        RuleFor(x => x.InstallmentId)
            .GreaterThan(0)
            .WithMessage("Installment ID must be greater than zero");

        RuleFor(x => x.AmountPaid)
            .GreaterThan(0)
            .WithMessage("Amount paid must be greater than zero")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("Amount paid cannot exceed 1,000,000");

        RuleFor(x => x.PaymentDate)
            .NotEmpty()
            .WithMessage("Payment date is required")
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Payment date cannot be in the future")
            .GreaterThan(DateTime.Today.AddYears(-5))
            .WithMessage("Payment date cannot be more than 5 years ago");

        RuleFor(x => x.TenantId)
            .GreaterThan(0)
            .WithMessage("Tenant ID must be greater than zero");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required")
            .Must(BeAllowedContentType)
            .WithMessage($"Content type must be one of: {string.Join(", ", AllowedContentTypes)}");

        RuleFor(x => x.FileContent)
            .NotEmpty()
            .WithMessage("File content is required")
            .Must(BeValidFileSize)
            .WithMessage($"File size cannot exceed {MaxFileSizeBytes / 1024 / 1024} MB");
    }

    private static bool BeAllowedContentType(string contentType)
    {
        return AllowedContentTypes.Contains(contentType.ToLowerInvariant());
    }

    private static bool BeValidFileSize(byte[] fileContent)
    {
        return fileContent.Length <= MaxFileSizeBytes;
    }
}

