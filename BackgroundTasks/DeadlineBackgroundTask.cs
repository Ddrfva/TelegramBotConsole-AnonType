using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.DataAccess;
using Core.Services;

namespace TelegramBot_31.BackgroundTasks
{
    public class DeadlineBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;

        public DeadlineBackgroundTask(
            INotificationService notificationService,
            IUserRepository userRepository,
            IToDoRepository toDoRepository)
            : base(TimeSpan.FromHours(1), nameof(DeadlineBackgroundTask))
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _toDoRepository = toDoRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var users = await _userRepository.GetUsers(ct);

            foreach (var user in users)
            {
                try
                {
                    var from = DateTime.UtcNow.AddDays(-1).Date;
                    var to = DateTime.UtcNow.Date;

                    var tasks = await _toDoRepository.GetActiveWithDeadline(user.Id, from, to, ct);

                    foreach (var task in tasks)
                    {
                        var type = $"Deadline_{task.Id}";
                        var text = $"Ой! Вы пропустили дедлайн по задаче {task.Name}";

                        await _notificationService.ScheduleNotification(
                            user.Id,
                            user.TelegramUserId,
                            type,
                            text,
                            DateTime.UtcNow,
                            ct);  // ← добавлен ct

                        Console.WriteLine($"⏰ Создана нотификация о дедлайне для задачи {task.Id} (пользователь {user.Id})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка в DeadlineBackgroundTask для пользователя {user.Id}: {ex.Message}");
                }
            }
        }
    }
}