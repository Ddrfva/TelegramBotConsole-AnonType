using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Services;
using Telegram.Bot;

namespace TelegramBot_31.BackgroundTasks
{
    public class NotificationBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly ITelegramBotClient _bot;

        public NotificationBackgroundTask(
            INotificationService notificationService,
            ITelegramBotClient bot)
            : base(TimeSpan.FromMinutes(1), nameof(NotificationBackgroundTask))
        {
            _notificationService = notificationService;
            _bot = bot;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            try
            {
                // Получаем нотификации, которые нужно отправить
                var notifications = await _notificationService.GetScheduledNotifications(DateTime.UtcNow, ct);

                Console.WriteLine($"📨 Найдено нотификаций для отправки: {notifications.Count}");

                foreach (var notification in notifications)
                {
                    try
                    {
                        Console.WriteLine($"📤 Отправка нотификации {notification.Id}");
                        Console.WriteLine($"   Пользователь (TelegramUserId): {notification.TelegramUserId}");
                        Console.WriteLine($"   Текст: {notification.Text}");

                        // Отправляем сообщение пользователю
                        await _bot.SendMessage(
                            chatId: notification.TelegramUserId,
                            text: notification.Text,
                            cancellationToken: ct);

                        Console.WriteLine($"✅ Сообщение отправлено в Telegram");

                        // Помечаем как отправленное
                        await _notificationService.MarkNotified(notification.Id, ct);

                        Console.WriteLine($"✅ Нотификация {notification.Id} помечена как отправленная");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Ошибка отправки нотификации {notification.Id}: {ex.Message}");
                        Console.WriteLine($"   Stack: {ex.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка в NotificationBackgroundTask: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}