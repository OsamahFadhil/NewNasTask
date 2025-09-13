using Insight.Invoicing.Application.DTOs;

namespace Insight.Invoicing.Application.Services;

public interface INotificationService
{
    Task SendToUserAsync(int userId, RealTimeNotificationDto notification, CancellationToken cancellationToken = default);

    Task SendToAdministratorsAsync(RealTimeNotificationDto notification, CancellationToken cancellationToken = default);

    Task SendToTenantAsync(int tenantId, RealTimeNotificationDto notification, CancellationToken cancellationToken = default);

    Task SendToContractParticipantsAsync(int contractId, RealTimeNotificationDto notification, CancellationToken cancellationToken = default);

    Task<NotificationDto> CreateNotificationAsync(NotificationDto notification, CancellationToken cancellationToken = default);

    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);

    Task DeleteOldNotificationsAsync(DateTime olderThan, CancellationToken cancellationToken = default);
}


