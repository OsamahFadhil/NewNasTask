using Insight.Invoicing.Application.DTOs;
using Insight.Invoicing.Application.Services;
using Insight.Invoicing.Domain.Entities;
using Insight.Invoicing.Domain.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Insight.Invoicing.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<Hub> _hubContext;
    private readonly IRepository<Notification> _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<Hub> hubContext,
        IRepository<Notification> notificationRepository,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task SendToUserAsync(int userId, RealTimeNotificationDto notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogInformation("Sent real-time notification to user {UserId}: {Title}", userId, notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send real-time notification to user {UserId}", userId);
        }
    }

    public async Task SendToAdministratorsAsync(RealTimeNotificationDto notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group("Administrators")
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogInformation("Sent real-time notification to administrators: {Title}", notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send real-time notification to administrators");
        }
    }

    public async Task SendToTenantAsync(int tenantId, RealTimeNotificationDto notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group($"Tenant_{tenantId}")
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogInformation("Sent real-time notification to tenant {TenantId}: {Title}", tenantId, notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send real-time notification to tenant {TenantId}", tenantId);
        }
    }

    public async Task SendToContractParticipantsAsync(int contractId, RealTimeNotificationDto notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients.Group($"Contract_{contractId}")
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogInformation("Sent real-time notification to contract {ContractId} participants: {Title}", contractId, notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send real-time notification to contract {ContractId} participants", contractId);
        }
    }

    public async Task<NotificationDto> CreateNotificationAsync(NotificationDto notificationDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new Notification(
                notificationDto.UserId!.Value,
                notificationDto.Title,
                notificationDto.Message,
                notificationDto.Type.ToString(),
                notificationDto.Priority.ToString(),
                notificationDto.ContractId,
                notificationDto.InstallmentId,
                notificationDto.PaymentReceiptId,
                notificationDto.Data.Any() ? JsonSerializer.Serialize(notificationDto.Data) : null
            );

            await _notificationRepository.AddAsync(notification, cancellationToken);

            _logger.LogInformation("Created notification {NotificationId} for user {UserId}", notification.Id, notification.UserId);

            return MapToDto(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification for user {UserId}", notificationDto.UserId);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var notifications = await _notificationRepository.GetAllAsync(cancellationToken);

            var userNotifications = notifications
                .Where(n => n.UserId == userId)
                .Where(n => !unreadOnly || !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50); // Limit to last 50 notifications

            return userNotifications.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications for user {UserId}", userId);
            throw;
        }
    }

    public async Task MarkAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _notificationRepository.GetSingleAsync(
                n => n.Id == notificationId && n.UserId == userId,
                cancellationToken);

            if (notification != null)
            {
                notification.MarkAsRead();
                await _notificationRepository.UpdateAsync(notification, cancellationToken);

                _logger.LogInformation("Marked notification {NotificationId} as read for user {UserId}", notificationId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification {NotificationId} as read for user {UserId}", notificationId, userId);
            throw;
        }
    }

    public async Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var notifications = await _notificationRepository.GetAllAsync(cancellationToken);
            var userNotifications = notifications.Where(n => n.UserId == userId && !n.IsRead);

            foreach (var notification in userNotifications)
            {
                notification.MarkAsRead();
                await _notificationRepository.UpdateAsync(notification, cancellationToken);
            }

            _logger.LogInformation("Marked all notifications as read for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}", userId);
            throw;
        }
    }

    public async Task DeleteOldNotificationsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        try
        {
            var notifications = await _notificationRepository.GetAllAsync(cancellationToken);
            var oldNotifications = notifications.Where(n => n.CreatedAt < olderThan);

            foreach (var notification in oldNotifications)
            {
                await _notificationRepository.RemoveByIdAsync(notification.Id, cancellationToken);
            }

            _logger.LogInformation("Deleted {Count} old notifications older than {Date}", oldNotifications.Count(), olderThan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete old notifications");
            throw;
        }
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        var data = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(notification.Data))
        {
            try
            {
                data = JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Data) ?? new();
            }
            catch
            {
            }
        }

        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = Enum.Parse<NotificationType>(notification.Type),
            UserId = notification.UserId,
            ContractId = notification.ContractId,
            InstallmentId = notification.InstallmentId,
            PaymentReceiptId = notification.PaymentReceiptId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            Data = data
        };
    }
}
