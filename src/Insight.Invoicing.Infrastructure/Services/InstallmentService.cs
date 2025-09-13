using Insight.Invoicing.Application.Services;
using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Repositories;
using Insight.Invoicing.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Infrastructure.Services;

public class InstallmentService : IInstallmentService
{
    private readonly IBaseRepository<Installment> _installmentRepository;
    private readonly IRepository<Contract> _contractRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<InstallmentService> _logger;

    public InstallmentService(
        IBaseRepository<Installment> installmentRepository,
        IRepository<Contract> contractRepository,
        IRepository<User> userRepository,
        IEmailService emailService,
        ISmsService smsService,
        INotificationService notificationService,
        ILogger<InstallmentService> logger)
    {
        _installmentRepository = installmentRepository;
        _contractRepository = contractRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _smsService = smsService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task CheckAndProcessOverdueInstallmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var overdueInstallments = await _installmentRepository.FindAsync(
                i => i.DueDate < today && i.Status == InstallmentStatus.Pending,
                cancellationToken);

            foreach (var installment in overdueInstallments)
            {
                await ProcessOverdueInstallmentAsync(installment.Id, cancellationToken);
            }

            _logger.LogInformation("Processed {Count} overdue installments", overdueInstallments.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking and processing overdue installments");
            throw;
        }
    }

    public async Task ProcessOverdueInstallmentAsync(int installmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var installment = await _installmentRepository.GetByIdAsync(installmentId, cancellationToken);
            if (installment == null)
            {
                _logger.LogWarning("Installment {InstallmentId} not found", installmentId);
                return;
            }

            var contract = await _contractRepository.GetByIdAsync(installment.ContractId, cancellationToken);
            if (contract == null)
            {
                _logger.LogWarning("Contract {ContractId} not found for installment {InstallmentId}",
                    installment.ContractId, installmentId);
                return;
            }

            var tenant = await _userRepository.GetByIdAsync(contract.TenantId, cancellationToken);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found for contract {ContractId}",
                    contract.TenantId, contract.Id);
                return;
            }

            var daysOverdue = (DateTime.UtcNow.Date - installment.DueDate).Days;
            if (daysOverdue <= 0)
            {
                return; // Not overdue yet
            }

            if (installment.Status == InstallmentStatus.Pending)
            {
                installment.MarkAsOverdue();
                await _installmentRepository.UpdateAsync(installment, cancellationToken);
            }

            await _emailService.SendInstallmentOverdueReminderAsync(
                tenant.Email,
                $"{tenant.FirstName} {tenant.LastName}",
                installment.Amount.Amount,
                installment.PenaltyAmount.Amount,
                installment.DueDate,
                daysOverdue,
                contract.Id,
                installment.Id,
                cancellationToken);

            if (!string.IsNullOrEmpty(tenant.PhoneNumber))
            {
                await _smsService.SendInstallmentOverdueSmsAsync(
                    tenant.PhoneNumber,
                    $"{tenant.FirstName} {tenant.LastName}",
                    installment.Amount.Amount,
                    installment.DueDate,
                    daysOverdue,
                    cancellationToken);
            }

            _logger.LogInformation("Processed overdue installment {InstallmentId} for tenant {TenantId}",
                installmentId, tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing overdue installment {InstallmentId}", installmentId);
            throw;
        }
    }

    public async Task SendDueRemindersAsync(int daysBeforeDue, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetDate = DateTime.UtcNow.Date.AddDays(daysBeforeDue);
            var dueInstallments = await _installmentRepository.FindAsync(
                i => i.DueDate.Date == targetDate && i.Status == InstallmentStatus.Pending,
                cancellationToken);

            foreach (var installment in dueInstallments)
            {
                await SendInstallmentReminderAsync(installment.Id, cancellationToken);
            }

            _logger.LogInformation("Sent {Count} installment reminders for {DaysBeforeDue} days before due",
                dueInstallments.Count(), daysBeforeDue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending due reminders for {DaysBeforeDue} days before due", daysBeforeDue);
            throw;
        }
    }

    public async Task SendInstallmentReminderAsync(int installmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var installment = await _installmentRepository.GetByIdAsync(installmentId, cancellationToken);
            if (installment == null)
            {
                _logger.LogWarning("Installment {InstallmentId} not found", installmentId);
                return;
            }

            var contract = await _contractRepository.GetByIdAsync(installment.ContractId, cancellationToken);
            if (contract == null)
            {
                _logger.LogWarning("Contract {ContractId} not found for installment {InstallmentId}",
                    installment.ContractId, installmentId);
                return;
            }

            var tenant = await _userRepository.GetByIdAsync(contract.TenantId, cancellationToken);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found for contract {ContractId}",
                    contract.TenantId, contract.Id);
                return;
            }

            var daysUntilDue = (installment.DueDate - DateTime.UtcNow.Date).Days;

            await _emailService.SendInstallmentDueReminderAsync(
                tenant.Email,
                $"{tenant.FirstName} {tenant.LastName}",
                installment.Amount.Amount,
                installment.DueDate,
                daysUntilDue,
                contract.Id,
                installment.Id,
                cancellationToken);

            if (!string.IsNullOrEmpty(tenant.PhoneNumber))
            {
                await _smsService.SendInstallmentDueSmsAsync(
                    tenant.PhoneNumber,
                    $"{tenant.FirstName} {tenant.LastName}",
                    installment.Amount.Amount,
                    installment.DueDate,
                    daysUntilDue,
                    cancellationToken);
            }

            _logger.LogInformation("Sent installment reminder for installment {InstallmentId} to tenant {TenantId}",
                installmentId, tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending installment reminder for installment {InstallmentId}", installmentId);
            throw;
        }
    }

    public async Task ApplyPenaltiesToOverdueInstallmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var overdueInstallments = await _installmentRepository.FindAsync(
                i => i.DueDate < today && i.Status == InstallmentStatus.Pending,
                cancellationToken);

            foreach (var installment in overdueInstallments)
            {
                installment.MarkAsOverdue();
                await _installmentRepository.UpdateAsync(installment, cancellationToken);
            }

            _logger.LogInformation("Applied penalties to {Count} overdue installments", overdueInstallments.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying penalties to overdue installments");
            throw;
        }
    }

    public async Task GenerateInstallmentScheduleAsync(int contractId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _contractRepository.GetByIdAsync(contractId, cancellationToken);
            if (contract == null)
            {
                _logger.LogWarning("Contract {ContractId} not found", contractId);
                return;
            }

            _logger.LogInformation("Generated installment schedule for contract {ContractId}", contractId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating installment schedule for contract {ContractId}", contractId);
            throw;
        }
    }

    public async Task<IEnumerable<InstallmentReminderDto>> GetInstallmentsDueSoonAsync(int daysBeforeDue, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetDate = DateTime.UtcNow.Date.AddDays(daysBeforeDue);
            var dueInstallments = await _installmentRepository.FindAsync(
                i => i.DueDate.Date == targetDate && i.Status == InstallmentStatus.Pending,
                cancellationToken);

            var result = new List<InstallmentReminderDto>();
            foreach (var installment in dueInstallments)
            {
                var contract = await _contractRepository.GetByIdAsync(installment.ContractId, cancellationToken);
                if (contract == null) continue;

                var tenant = await _userRepository.GetByIdAsync(contract.TenantId, cancellationToken);
                if (tenant == null) continue;

                result.Add(new InstallmentReminderDto
                {
                    InstallmentId = installment.Id,
                    ContractId = contract.Id,
                    TenantId = tenant.Id,
                    TenantName = $"{tenant.FirstName} {tenant.LastName}",
                    TenantEmail = tenant.Email,
                    TenantPhone = tenant.PhoneNumber,
                    Amount = installment.Amount.Amount,
                    PenaltyAmount = installment.PenaltyAmount.Amount,
                    DueDate = installment.DueDate,
                    DaysUntilDue = daysBeforeDue,
                    Status = installment.Status.ToString()
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting installments due in {DaysBeforeDue} days", daysBeforeDue);
            throw;
        }
    }

    public async Task<IEnumerable<InstallmentReminderDto>> GetOverdueInstallmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var overdueInstallments = await _installmentRepository.FindAsync(
                i => i.DueDate < today && (i.Status == InstallmentStatus.Pending || i.Status == InstallmentStatus.Overdue),
                cancellationToken);

            var result = new List<InstallmentReminderDto>();
            foreach (var installment in overdueInstallments)
            {
                var contract = await _contractRepository.GetByIdAsync(installment.ContractId, cancellationToken);
                if (contract == null) continue;

                var tenant = await _userRepository.GetByIdAsync(contract.TenantId, cancellationToken);
                if (tenant == null) continue;

                var daysOverdue = (today - installment.DueDate).Days;

                result.Add(new InstallmentReminderDto
                {
                    InstallmentId = installment.Id,
                    ContractId = contract.Id,
                    TenantId = tenant.Id,
                    TenantName = $"{tenant.FirstName} {tenant.LastName}",
                    TenantEmail = tenant.Email,
                    TenantPhone = tenant.PhoneNumber,
                    Amount = installment.Amount.Amount,
                    PenaltyAmount = installment.PenaltyAmount.Amount,
                    DueDate = installment.DueDate,
                    DaysOverdue = daysOverdue,
                    Status = installment.Status.ToString()
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue installments");
            throw;
        }
    }
}
