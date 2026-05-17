using Core.Entities;
using Core.Services;
using Core.Exceptions;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace TelegramBot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _reportService;

        public UpdateHandler(IUserService userService, IToDoService todoService, IToDoReportService reportService)
        {
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            if (update.Message?.Text == null)
                return;

            var messageText = update.Message.Text.Trim();
            var chat = update.Message.Chat;
            var telegramUserId = update.Message.From?.Id ?? 0;
            var telegramUserName = update.Message.From?.Username ?? "User";

            var user = _userService.GetUser(telegramUserId);
            if (user == null)
            {
                user = _userService.RegisterUser(telegramUserId, telegramUserName);
                botClient.SendMessage(chat, $"Добро пожаловать, {telegramUserName}! Вы зарегистрированы.");
                return;
            }

            if (messageText == "/start")
            {
                botClient.SendMessage(chat, $"С возвращением, {user.TelegramUserName}! Введите /help для списка команд.");
                return;
            }

            if (messageText == "/help")
            {
                var helpText = "Доступные команды:\n" +
                               "/start - начать работу\n" +
                               "/help - показать справку\n" +
                               "/info - информация о программе\n" +
                               "/addtask [текст] - добавить задачу\n" +
                               "/showtasks - показать активные задачи\n" +
                               "/showalltasks - показать все задачи\n" +
                               "/completetask [id] - завершить задачу по Id\n" +
                               "/removetask [номер] - удалить задачу по номеру\n" +
                               "/report - статистика по задачам\n" +
                               "/find [префикс] - найти задачи по началу названия\n" +
                               "/exit - выход";
                botClient.SendMessage(chat, helpText);
                return;
            }

            if (messageText == "/info")
            {
                var infoText = "Консольный бот для управления задачами\nВерсия: 7.0.0\nАвтор: Dorofeeva Daria";
                botClient.SendMessage(chat, infoText);
                return;
            }

            if (messageText.StartsWith("/addtask "))
            {
                var taskName = messageText.Substring(9).Trim();
                if (string.IsNullOrWhiteSpace(taskName))
                {
                    botClient.SendMessage(chat, "Ошибка: укажите название задачи. Пример: /addtask Купить хлеб");
                    return;
                }

                try
                {
                    var newTask = _todoService.Add(user, taskName);
                    botClient.SendMessage(chat, $"Задача \"{taskName}\" добавлена. Id: {newTask.Id}");
                }
                catch (Exception ex)
                {
                    botClient.SendMessage(chat, $"Ошибка: {ex.Message}");
                }
                return;
            }

            if (messageText == "/showtasks")
            {
                var activeTasks = _todoService.GetActiveByUserId(user.UserId);
                if (activeTasks.Count == 0)
                {
                    botClient.SendMessage(chat, "Список активных задач пуст.");
                    return;
                }

                var result = "Ваши активные задачи:\n";
                for (int i = 0; i < activeTasks.Count; i++)
                {
                    var task = activeTasks[i];
                    result += $"{i + 1}. {task.Name} - {task.CreatedAtUtc.ToLocalTime():dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
                }
                botClient.SendMessage(chat, result);
                return;
            }

            if (messageText == "/showalltasks")
            {
                var allTasks = _todoService.GetAllByUserId(user.UserId);
                if (allTasks.Count == 0)
                {
                    botClient.SendMessage(chat, "Список задач пуст.");
                    return;
                }

                var result = "Все задачи:\n";
                foreach (var task in allTasks)
                {
                    var state = task.State == ToDoItemState.Active ? "Active" : "Completed";
                    result += $"{state} - {task.Name} - {task.CreatedAtUtc.ToLocalTime():dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
                }
                botClient.SendMessage(chat, result);
                return;
            }

            if (messageText.StartsWith("/completetask "))
            {
                var guidString = messageText.Substring(14).Trim();
                if (!Guid.TryParse(guidString, out Guid taskId))
                {
                    botClient.SendMessage(chat, "Ошибка: неверный формат Id.");
                    return;
                }

                try
                {
                    _todoService.MarkCompleted(taskId);
                    botClient.SendMessage(chat, $"Задача с Id {taskId} завершена.");
                }
                catch (Exception ex)
                {
                    botClient.SendMessage(chat, $"Ошибка: {ex.Message}");
                }
                return;
            }

            if (messageText.StartsWith("/removetask "))
            {
                var numberString = messageText.Substring(12).Trim();
                if (!int.TryParse(numberString, out int taskNumber))
                {
                    botClient.SendMessage(chat, "Ошибка: укажите номер задачи. Пример: /removetask 2");
                    return;
                }

                var activeTasks = _todoService.GetActiveByUserId(user.UserId);
                if (taskNumber < 1 || taskNumber > activeTasks.Count)
                {
                    botClient.SendMessage(chat, $"Ошибка: введите номер от 1 до {activeTasks.Count}.");
                    return;
                }

                var taskToRemove = activeTasks[taskNumber - 1];
                _todoService.Delete(taskToRemove.Id);
                botClient.SendMessage(chat, $"Задача \"{taskToRemove.Name}\" удалена.");
                return;
            }

            if (messageText == "/report")
            {
                var stats = _reportService.GetUserStats(user.UserId);
                botClient.SendMessage(chat, $"Статистика по задачам на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}. Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};");
                return;
            }

            if (messageText.StartsWith("/find "))
            {
                var prefix = messageText.Substring(6).Trim();
                if (string.IsNullOrWhiteSpace(prefix))
                {
                    botClient.SendMessage(chat, "Ошибка: укажите начало названия задачи. Пример: /find Купить");
                    return;
                }

                var foundTasks = _todoService.Find(user, prefix);
                if (foundTasks.Count == 0)
                {
                    botClient.SendMessage(chat, "Задачи, начинающиеся с указанного префикса, не найдены.");
                    return;
                }

                var result = "Найденные задачи:\n";
                for (int i = 0; i < foundTasks.Count; i++)
                {
                    var task = foundTasks[i];
                    var state = task.State == ToDoItemState.Active ? "Active" : "Completed";
                    result += $"{i + 1}. {state} - {task.Name} - {task.CreatedAtUtc.ToLocalTime():dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
                }
                botClient.SendMessage(chat, result);
                return;
            }

            if (messageText == "/exit")
            {
                botClient.SendMessage(chat, "До свидания!");
                return;
            }

            botClient.SendMessage(chat, "Неизвестная команда. Введите /help для списка команд.");
        }
    }
}