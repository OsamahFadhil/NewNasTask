using Insight.Invoicing.Application.IntegrationEvents;
using Insight.Invoicing.Application.Services;
using Insight.Invoicing.Application.DTOs;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Infrastructure.EventHandlers.IntegrationEventConsumers;

public class ContractApprovedConsumer : IConsumer<ContractApprovedIntegrationEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ContractApprovedConsumer> _logger;

    public ContractApprovedConsumer(
        INotificationService notificationService,
        IEmailService emailService,
        ILogger<ContractApprovedConsumer> logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ContractApprovedIntegrationEvent> context)
    {
        var integrationEvent = context.Message;

        _logger.LogInformation("Processing ContractApprovedIntegrationEvent for contract {ContractId}",
            integrationEvent.ContractId);

        try
        {
            var realTimeNotification = new RealTimeNotificationDto
            {
                Type = NotificationType.ContractApproved.ToString(),
                Title = "Contract Approved",
                Message = $"Your contract for apartment {integrationEvent.ContractId} has been approved!",
                Priority = NotificationPriority.High,
                Data = new { contractId = integrationEvent.ContractId, approvedBy = integrationEvent.ApprovedByName },
                ActionUrl = $"/contracts/{integrationEvent.ContractId}"
            };

            await _notificationService.SendToTenantAsync(integrationEvent.TenantId, realTimeNotification);

            var persistentNotification = new NotificationDto
            {
                UserId = integrationEvent.TenantId,
                Title = "Contract Approved",
                Message = $"Your contract for apartment {integrationEvent.ContractId} has been approved by {integrationEvent.ApprovedByName}. Total amount: {integrationEvent.TotalAmount:C}. Installments: {integrationEvent.NumberOfInstallments}.",
                Type = NotificationType.ContractApproved,
                ContractId = integrationEvent.ContractId,
                Data = new Dictionary<string, object>
                {
                    ["contractId"] = integrationEvent.ContractId,
                    ["approvedBy"] = integrationEvent.ApprovedByName,
                    ["totalAmount"] = integrationEvent.TotalAmount,
                    ["numberOfInstallments"] = integrationEvent.NumberOfInstallments,
                    ["startDate"] = integrationEvent.StartDate,
                    ["endDate"] = integrationEvent.EndDate
                }
            };

            await _notificationService.CreateNotificationAsync(persistentNotification);

            await _emailService.SendContractApprovedEmailAsync(
                integrationEvent.TenantEmail,
                integrationEvent.TenantName,
                integrationEvent.ContractId,
                integrationEvent.TotalAmount,
                integrationEvent.NumberOfInstallments,
                integrationEvent.StartDate,
                integrationEvent.EndDate);

            _logger.LogInformation("Successfully processed ContractApprovedIntegrationEvent for contract {ContractId}",
                integrationEvent.ContractId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ContractApprovedIntegrationEvent for contract {ContractId}",
                integrationEvent.ContractId);
            throw;
        }
    }
}


