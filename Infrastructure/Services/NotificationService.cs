using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Entities;
using Core.Services;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Models;
using LinqToDB;
using LinqToDB.Data;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public NotificationService(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public async Task<bool> ScheduleNotification(
            Guid userId,
            long telegramUserId,
            string type,
            string text,
            DateTime scheduledAt,
            CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();

            var exists = await db.Notifications
                .AnyAsync(n => n.UserId == userId && n.Type == type);

            if (exists)
                return false;

            var notification = new NotificationModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TelegramUserId = telegramUserId,
                Type = type,
                Text = text,
                ScheduledAt = scheduledAt,
                IsNotified = false,
                NotifiedAt = null
            };

            await db.InsertAsync(notification);
            Console.WriteLine($"📝 Создана нотификация {notification.Id} (Type: {type}) для пользователя {userId}");
            return true;
        }

        public async Task<IReadOnlyList<Notification>> GetScheduledNotifications(
            DateTime scheduledBefore,
            CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();

            var models = await db.Notifications
                .Where(n => !n.IsNotified && n.ScheduledAt <= scheduledBefore)
                .ToListAsync();

            Console.WriteLine($"📋 Найдено в БД: {models.Count} нотификаций для отправки (ScheduledAt <= {scheduledBefore:yyyy-MM-dd HH:mm:ss})");

            foreach (var m in models)
            {
                Console.WriteLine($"  - {m.Type}: {m.Text} (TelegramUserId: {m.TelegramUserId}, ScheduledAt: {m.ScheduledAt:yyyy-MM-dd HH:mm:ss})");
            }

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task MarkNotified(Guid notificationId, CancellationToken ct)
        {
            using var db = _factory.CreateDataContext();

            await db.Notifications
                .Where(n => n.Id == notificationId)
                .Set(n => n.IsNotified, true)
                .Set(n => n.NotifiedAt, DateTime.UtcNow)
                .UpdateAsync();

            Console.WriteLine($"✅ Нотификация {notificationId} помечена как отправленная");
        }
    }
}