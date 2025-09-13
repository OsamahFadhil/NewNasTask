using Insight.Invoicing.Domain.Events;
using Insight.Invoicing.Domain.Repositories;
using Insight.Invoicing.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Insight.Invoicing.Infrastructure.Persistence;

namespace Insight.Invoicing.Infrastructure.EventHandlers;

public class ContractApprovedEventHandler : INotificationHandler<ContractApprovedEvent>
{
    private readonly IContractRepository _contractRepository;
    private readonly IInstallmentCalculatorService _installmentCalculator;
    private readonly ILogger<ContractApprovedEventHandler> _logger;
    private readonly AppDbContext _dbContext;

    public ContractApprovedEventHandler(
        IContractRepository contractRepository,
        IInstallmentCalculatorService installmentCalculator,
        ILogger<ContractApprovedEventHandler> logger,
        AppDbContext dbContext)
    {
        _contractRepository = contractRepository;
        _installmentCalculator = installmentCalculator;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Handle(ContractApprovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing ContractApprovedEvent for contract {ContractId}",
            notification.ContractId);

        try
        {
            var contract = await _contractRepository.GetWithInstallmentsAsync(
                notification.ContractId,
                cancellationToken);

            if (contract == null)
            {
                _logger.LogWarning(
                    "Contract {ContractId} not found while processing ContractApprovedEvent",
                    notification.ContractId);
                return;
            }

            var refreshedContract = await _contractRepository.GetWithInstallmentsAsync(
                notification.ContractId,
                cancellationToken);

            _logger.LogInformation(
                "Contract {ContractId} has {InstallmentCount} installments loaded from database",
                refreshedContract!.Id,
                refreshedContract.Installments.Count());

            var existingInstallmentsCount = await _dbContext.Installments
                .CountAsync(i => i.ContractId == notification.ContractId, cancellationToken);

            _logger.LogInformation(
                "Database shows {ExistingCount} installments for contract {ContractId}",
                existingInstallmentsCount,
                notification.ContractId);

            if (!refreshedContract.Installments.Any() && existingInstallmentsCount == 0)
            {
                _logger.LogInformation(
                    "Creating installments for contract {ContractId}",
                    refreshedContract.Id);

                try
                {
                    var installments = await _installmentCalculator.CalculateInstallmentScheduleAsync(
                        refreshedContract,
                        gracePeriodDays: 5);

                    refreshedContract.AddInstallments(installments);
                    await _contractRepository.UpdateAsync(refreshedContract, cancellationToken);

                    _logger.LogInformation(
                        "Created {InstallmentCount} installments for contract {ContractId}",
                        installments.Count(),
                        refreshedContract.Id);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true ||
                                                                              ex.InnerException?.Message.Contains("IX_Installments_Contract_Sequence") == true)
                {
                    _logger.LogWarning(
                        "Installments already exist in database for contract {ContractId}, skipping creation. Error: {Error}",
                        refreshedContract.Id,
                        ex.InnerException?.Message ?? ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Unexpected error creating installments for contract {ContractId}",
                        refreshedContract.Id);
                    throw;
                }
            }
            else
            {
                _logger.LogInformation(
                    "Installments already exist for contract {ContractId} (in-memory: {InMemoryCount}, database: {DbCount}), skipping creation",
                    refreshedContract.Id,
                    refreshedContract.Installments.Count(),
                    existingInstallmentsCount);
            }

            await SendContractApprovalEmailAsync(refreshedContract, notification, cancellationToken);

            _logger.LogInformation(
                "Successfully processed ContractApprovedEvent for contract {ContractId}",
                notification.ContractId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing ContractApprovedEvent for contract {ContractId}",
                notification.ContractId);
            throw;
        }
    }

    private async Task SendContractApprovalEmailAsync(
        Domain.Entities.Contract contract,
        ContractApprovedEvent notification,
        CancellationToken cancellationToken)
    {

        var tenantEmail = contract.Tenant?.Email ?? "Unknown";

        _logger.LogInformation(
            "Would send contract approval email to tenant {TenantId} ({TenantEmail}) for contract {ContractId}",
            contract.TenantId,
            tenantEmail,
            contract.Id);

        // var emailContent = await _templateService.GenerateContractApprovalEmailAsync(contract);
        // await _emailService.SendEmailAsync(tenantEmail, "Contract Approved", emailContent);

        await Task.CompletedTask;
    }
}

