using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.DataAccess;
using Core.Services;

namespace TelegramBot_31.BackgroundTasks
{
    public class TodayBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;

        public TodayBackgroundTask(
            INotificationService notificationService,
            IUserRepository userRepository,
            IToDoRepository toDoRepository)
            : base(TimeSpan.FromDays(1), nameof(TodayBackgroundTask))
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _toDoRepository = toDoRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            // Получаем всех пользователей
            var users = await _userRepository.GetUsers(ct);

            // Сегодняшняя дата
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var user in users)
            {
                try
                {
                    // Получаем задачи на сегодня
                    var allTasks = await _toDoRepository.GetAllByUserId(user.Id, ct);
                    var todayTasks = allTasks
                        .Where(t => t.Deadline.HasValue
                            && DateOnly.FromDateTime(t.Deadline.Value) == today
                            && t.State == 0)
                        .ToList();

                    if (!todayTasks.Any())
                        continue;

                    // Формируем текст
                    var text = new StringBuilder();
                    text.AppendLine($"📋 Задачи на сегодня ({today}):");

                    foreach (var task in todayTasks)
                    {
                        text.AppendLine($"  - {task.Name}");
                    }

                    // Создаём нотификацию (только одну на пользователя в день)
                    var type = $"Today_{today}";

                    await _notificationService.ScheduleNotification(
                        user.Id,
                        user.TelegramUserId,  // ← ДОБАВЛЕНО!
                        type,
                        text.ToString(),
                        DateTime.UtcNow,
                        ct);

                    Console.WriteLine($"📅 Создана нотификация 'Задачи на сегодня' для пользователя {user.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка в TodayBackgroundTask для пользователя {user.Id}: {ex.Message}");
                }
            }
        }
    }
}