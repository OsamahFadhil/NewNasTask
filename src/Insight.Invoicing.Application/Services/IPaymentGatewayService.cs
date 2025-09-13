namespace Insight.Invoicing.Application.Services;

public interface IPaymentGatewayService
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        CreatePaymentIntentRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentResult> ConfirmPaymentAsync(
        string paymentIntentId,
        string paymentMethodId,
        CancellationToken cancellationToken = default);

    Task<PaymentResult> GetPaymentStatusAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);

    Task<RefundResult> RefundPaymentAsync(
        string paymentIntentId,
        decimal? amount = null,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<WebhookResult> HandleWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default);

    Task<CustomerResult> CreateCustomerAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default);
}

public class CreatePaymentIntentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CustomerId { get; set; } = string.Empty;
    public int ContractId { get; set; }
    public int InstallmentId { get; set; }
    public int TenantId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class CreateCustomerRequest
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class PaymentIntentResult
{
    public bool IsSuccess { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ErrorMessage { get; set; }
    public PaymentStatus Status { get; set; }
}

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string? PaymentIntentId { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime? PaidAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class RefundResult
{
    public bool IsSuccess { get; set; }
    public string? RefundId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime RefundedAt { get; set; } = DateTime.UtcNow;
}

public class WebhookResult
{
    public bool IsSuccess { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? PaymentIntentId { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ShouldRetry { get; set; }
}

public class CustomerResult
{
    public bool IsSuccess { get; set; }
    public string? CustomerId { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum PaymentStatus
{
    RequiresPaymentMethod,
    RequiresConfirmation,
    RequiresAction,
    Processing,
    RequiresCapture,
    Canceled,
    Succeeded,
    Failed
}


