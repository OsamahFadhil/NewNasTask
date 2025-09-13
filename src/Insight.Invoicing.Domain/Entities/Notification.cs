using Insight.Invoicing.Shared.Common;

namespace Insight.Invoicing.Domain.Entities;

public class Notification : AggregateRoot
{
    public int UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public string Type { get; private set; } = string.Empty;

    public int? ContractId { get; private set; }

    public int? InstallmentId { get; private set; }

    public int? PaymentReceiptId { get; private set; }

    public bool IsRead { get; private set; }

    public DateTime? ReadAt { get; private set; }

    public string? Data { get; private set; }

    public string Priority { get; private set; } = "Normal";

    public User User { get; private set; } = null!;

    public Contract? Contract { get; private set; }

    public Installment? Installment { get; private set; }

    public PaymentReceipt? PaymentReceipt { get; private set; }

    private Notification() { }

    public Notification(
        int userId,
        string title,
        string message,
        string type,
        string priority = "Normal",
        int? contractId = null,
        int? installmentId = null,
        int? paymentReceiptId = null,
        string? data = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty", nameof(message));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type cannot be null or empty", nameof(type));

        UserId = userId;
        Title = title.Trim();
        Message = message.Trim();
        Type = type.Trim();
        Priority = priority;
        ContractId = contractId;
        InstallmentId = installmentId;
        PaymentReceiptId = paymentReceiptId;
        Data = data;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
            UpdateTimestamp();
        }
    }

    public void MarkAsUnread()
    {
        if (IsRead)
        {
            IsRead = false;
            ReadAt = null;
            UpdateTimestamp();
        }
    }

    public void UpdateData(string? data)
    {
        Data = data;
        UpdateTimestamp();
    }
}
