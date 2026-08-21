using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Services
{
    public interface INotificationService
    {
        Task<bool> ScheduleNotification(
            Guid userId,
            long telegramUserId,  // ← ДОБАВЛЕНО
            string type,
            string text,
            DateTime scheduledAt,
            CancellationToken ct);

        Task<IReadOnlyList<Notification>> GetScheduledNotifications(
            DateTime scheduledBefore,
            CancellationToken ct);

        Task MarkNotified(Guid notificationId, CancellationToken ct);
    }
}