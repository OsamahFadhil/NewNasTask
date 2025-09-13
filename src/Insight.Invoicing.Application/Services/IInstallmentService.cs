namespace Insight.Invoicing.Application.Services;

public interface IInstallmentService
{
    Task CheckAndProcessOverdueInstallmentsAsync(CancellationToken cancellationToken = default);

    Task ProcessOverdueInstallmentAsync(int installmentId, CancellationToken cancellationToken = default);

    Task SendDueRemindersAsync(int daysBeforeDue, CancellationToken cancellationToken = default);

    Task SendInstallmentReminderAsync(int installmentId, CancellationToken cancellationToken = default);

    Task ApplyPenaltiesToOverdueInstallmentsAsync(CancellationToken cancellationToken = default);

    Task GenerateInstallmentScheduleAsync(int contractId, CancellationToken cancellationToken = default);

    Task<IEnumerable<InstallmentReminderDto>> GetInstallmentsDueSoonAsync(int daysBeforeDue, CancellationToken cancellationToken = default);

    Task<IEnumerable<InstallmentReminderDto>> GetOverdueInstallmentsAsync(CancellationToken cancellationToken = default);
}

public class InstallmentReminderDto
{
    public int InstallmentId { get; set; }
    public int ContractId { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string TenantEmail { get; set; } = string.Empty;
    public string TenantPhone { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PenaltyAmount { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public int DaysUntilDue { get; set; }
    public string Status { get; set; } = string.Empty;
}


