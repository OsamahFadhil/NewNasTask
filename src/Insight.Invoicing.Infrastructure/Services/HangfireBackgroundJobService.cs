using Hangfire;
using Insight.Invoicing.Application.Services;
using Microsoft.Extensions.Logging;

namespace Insight.Invoicing.Infrastructure.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly ILogger<HangfireBackgroundJobService> _logger;

    public HangfireBackgroundJobService(ILogger<HangfireBackgroundJobService> logger)
    {
        _logger = logger;
    }

    public void ScheduleOutboxProcessing()
    {
        RecurringJob.AddOrUpdate<IOutboxService>(
            "process-outbox-events",
            service => service.ProcessPendingEventsAsync(CancellationToken.None),
            "*/1 * * * *"); // Every minute

        RecurringJob.AddOrUpdate<IOutboxService>(
            "retry-failed-outbox-events",
            service => service.RetryFailedEventsAsync(CancellationToken.None),
            "*/5 * * * *"); // Every 5 minutes

        _logger.LogInformation("Scheduled outbox processing jobs");
    }

    public void ScheduleOverdueInstallmentCheck()
    {
        RecurringJob.AddOrUpdate<IInstallmentService>(
            "check-overdue-installments",
            service => service.CheckAndProcessOverdueInstallmentsAsync(CancellationToken.None),
            "0 9 * * *"); // Daily at 9 AM

        _logger.LogInformation("Scheduled overdue installment check job");
    }

    public void ScheduleInstallmentReminders()
    {
        RecurringJob.AddOrUpdate<IInstallmentService>(
            "send-7-day-reminders",
            service => service.SendDueRemindersAsync(7, CancellationToken.None),
            "0 10 * * *"); // Daily at 10 AM

        RecurringJob.AddOrUpdate<IInstallmentService>(
            "send-3-day-reminders",
            service => service.SendDueRemindersAsync(3, CancellationToken.None),
            "0 10 * * *"); // Daily at 10 AM

        RecurringJob.AddOrUpdate<IInstallmentService>(
            "send-1-day-reminders",
            service => service.SendDueRemindersAsync(1, CancellationToken.None),
            "0 10 * * *"); // Daily at 10 AM

        _logger.LogInformation("Scheduled installment reminder jobs");
    }

    public void ScheduleDataCleanup()
    {
        // Clean up old outbox events (older than 30 days)
        RecurringJob.AddOrUpdate<IOutboxService>(
            "cleanup-old-outbox-events",
            service => service.CleanupProcessedEventsAsync(DateTime.UtcNow.AddDays(-30), CancellationToken.None),
            "0 2 * * 0"); // Weekly on Sunday at 2 AM

        // Clean up old notifications (older than 90 days)
        RecurringJob.AddOrUpdate<INotificationService>(
            "cleanup-old-notifications",
            service => service.DeleteOldNotificationsAsync(DateTime.UtcNow.AddDays(-90), CancellationToken.None),
            "0 3 * * 0"); // Weekly on Sunday at 3 AM

        _logger.LogInformation("Scheduled data cleanup jobs");
    }

    public string ScheduleInstallmentReminderJob(int installmentId, DateTime scheduleAt)
    {
        var jobId = BackgroundJob.Schedule<IInstallmentService>(
            service => service.SendInstallmentReminderAsync(installmentId, CancellationToken.None),
            scheduleAt);

        _logger.LogInformation("Scheduled installment reminder job {JobId} for installment {InstallmentId} at {ScheduleAt}",
            jobId, installmentId, scheduleAt);

        return jobId;
    }

    public string ScheduleOverdueNotificationJob(int installmentId, DateTime scheduleAt)
    {
        var jobId = BackgroundJob.Schedule<IInstallmentService>(
            service => service.ProcessOverdueInstallmentAsync(installmentId, CancellationToken.None),
            scheduleAt);

        _logger.LogInformation("Scheduled overdue notification job {JobId} for installment {InstallmentId} at {ScheduleAt}",
            jobId, installmentId, scheduleAt);

        return jobId;
    }

    public void CancelJob(string jobId)
    {
        BackgroundJob.Delete(jobId);
        _logger.LogInformation("Cancelled background job {JobId}", jobId);
    }

    public string ExecuteBackgroundJob<T>(System.Linq.Expressions.Expression<Func<T, Task>> methodCall)
    {
        var jobId = BackgroundJob.Enqueue(methodCall);
        _logger.LogInformation("Enqueued background job {JobId}", jobId);
        return jobId;
    }
}


