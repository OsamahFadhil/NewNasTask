namespace Insight.Invoicing.Application.Services;

public interface IBackgroundJobService
{
    void ScheduleOutboxProcessing();

    void ScheduleOverdueInstallmentCheck();

    void ScheduleInstallmentReminders();

    void ScheduleDataCleanup();

    string ScheduleInstallmentReminderJob(int installmentId, DateTime scheduleAt);

    string ScheduleOverdueNotificationJob(int installmentId, DateTime scheduleAt);

    void CancelJob(string jobId);

    string ExecuteBackgroundJob<T>(System.Linq.Expressions.Expression<Func<T, Task>> methodCall);
}


