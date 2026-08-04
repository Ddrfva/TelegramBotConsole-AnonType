using Core.Entities;
using Core.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace TelegramBotConsole_Async
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

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message?.Text == null)
                    return;

                var messageText = update.Message.Text.Trim();
                var chat = update.Message.Chat;
                var telegramUserId = update.Message.From?.Id ?? 0;
                var telegramUserName = update.Message.From?.Username ?? "User";

                var user = await _userService.GetUser(telegramUserId, cancellationToken);
                if (user == null)
                {
                    user = await _userService.RegisterUser(telegramUserId, telegramUserName, cancellationToken);
                    await botClient.SendMessage(chat, $"Добро пожаловать, {telegramUserName}! Вы зарегистрированы.", cancellationToken);
                    return;
                }

                if (messageText == "/start")
                {
                    await botClient.SendMessage(chat, $"С возвращением, {user.TelegramUserName}! Введите /help для списка команд.", cancellationToken);
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
                    await botClient.SendMessage(chat, helpText, cancellationToken);
                    return;
                }

                if (messageText == "/info")
                {
                    var infoText = "Консольный бот для управления задачами\nВерсия: 7.0.0\nАвтор: Dorofeeva Daria";
                    await botClient.SendMessage(chat, infoText, cancellationToken);
                    return;
                }

                if (messageText.StartsWith("/addtask "))
                {
                    var taskName = messageText.Substring(9).Trim();
                    if (string.IsNullOrWhiteSpace(taskName))
                    {
                        await botClient.SendMessage(chat, "Ошибка: укажите название задачи. Пример: /addtask Купить хлеб", cancellationToken);
                        return;
                    }

                    try
                    {
                        var newTask = await _todoService.Add(user, taskName, cancellationToken);
                        await botClient.SendMessage(chat, $"Задача \"{taskName}\" добавлена. Id: {newTask.Id}", cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chat, $"Ошибка: {ex.Message}", cancellationToken);
                    }
                    return;
                }

                if (messageText == "/showtasks")
                {
                    var activeTasks = await _todoService.GetActiveByUserId(user.UserId, cancellationToken);
                    if (activeTasks.Count == 0)
                    {
                        await botClient.SendMessage(chat, "Список активных задач пуст.", cancellationToken);
                        return;
                    }

                    var result = "Ваши активные задачи:\n";
                    for (int i = 0; i < activeTasks.Count; i++)
                    {
                        var task = activeTasks[i];
                        result += $"{i + 1}. {task.Name} - {task.CreatedAtUtc.ToLocalTime():dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
                    }
                    await botClient.SendMessage(chat, result, cancellationToken);
                    return;
                }

                if (messageText == "/showalltasks")
                {
                    var allTasks = await _todoService.GetAllByUserId(user.UserId, cancellationToken);
                    if (allTasks.Count == 0)
                    {
                        await botClient.SendMessage(chat, "Список задач пуст.", cancellationToken);
                        return;
                    }

                    var result = "Все задачи:\n";
                    foreach (var task in allTasks)
                    {
                        var state = task.State == ToDoItemState.Active ? "Active" : "Completed";
                        result += $"{state} - {task.Name} - {task.CreatedAtUtc.ToLocalTime():dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
                    }
                    await botClient.SendMessage(chat, result, cancellationToken);
                    return;
                }

                if (messageText.StartsWith("/completetask "))
                {
                    var guidString = messageText.Substring(14).Trim();
                    if (!Guid.TryParse(guidString, out Guid taskId))
                    {
                        await botClient.SendMessage(chat, "Ошибка: неверный формат Id.", cancellationToken);
                        return;
                    }

                    try
                    {
                        await _todoService.MarkCompleted(taskId, cancellationToken);
                        await botClient.SendMessage(chat, $"Задача с Id {taskId} завершена.", cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chat, $"Ошибка: {ex.Message}", cancellationToken);
                    }
                    return;
                }

                if (messageText.StartsWith("/removetask "))
                {
                    var numberString = messageText.Substring(12).Trim();
                    if (!int.TryParse(numberString, out int taskNumber))
                    {
                        await botClient.SendMessage(chat, "Ошибка: укажите номер задачи. Пример: /removetask 2", cancellationToken);
                        return;
                    }

                    var activeTasks = await _todoService.GetActiveByUserId(user.UserId, cancellationToken);
                    if (taskNumber < 1 || taskNumber > activeTasks.Count)
                    {
                        await botClient.SendMessage(chat, $"Ошибка: введите номер от 1 до {activeTasks.Count}.", cancellationToken);
                        return;
                    }

                    var taskToRemove = activeTasks[taskNumber - 1];
                    await _todoService.Delete(taskToRemove.Id, cancellationToken);
                    await botClient.SendMessage(chat, $"Задача \"{taskToRemove.Name}\" удалена.", cancellationToken);
                    return;
                }

                if (messageText == "/report")
                {
                    var stats = await _reportService.GetUserStats(user.UserId, cancellationToken);
                    await botClient.SendMessage(chat, $"Статистика по задачам на {stats.generatedAt:dd.MM.yyyy HH:mm:ss}. Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};", cancellationToken);
                    return;
                }

                if (messageText.StartsWith("/find "))
                {
                    var prefix = messageText.Substring(6).Trim();
                    if (string.IsNullOrWhiteSpace(prefix))
                    {
                        await botClient.SendMessage(chat, "Ошибка: укажите начало названия задачи. Пример: /find Купить", cancellationToken);
                        return;
                    }

                    var foundTasks = await _todoService.Find(user, prefix, cancellationToken);
                    if (foundTasks.Count == 0)
                    {
                        await botClient.SendMessage(chat, "Задачи, начинающиеся с указанного префикса, не найдены.", cancellationToken);
                        return;
                    }

                    var result = "Найденные задачи:\n";
                    for (int i = 0; i < foundTasks.Count; i++)
                    {
                        var task = foundTasks[i];
                        var state = task.State == ToDoItemState.Active ? "Active" : "Completed";
                        result += $"{i + 1}. {state} - {task.Name} - {task.CreatedAtUtc.ToLocalTime():dd.MM.yyyy HH:mm:ss} - {task.Id}\n";
                    }
                    await botClient.SendMessage(chat, result, cancellationToken);
                    return;
                }

                if (messageText == "/exit")
                {
                    await botClient.SendMessage(chat, "До свидания!", cancellationToken);
                    return;
                }

                await botClient.SendMessage(chat, "Неизвестная команда. Введите /help для списка команд.", cancellationToken);
            }
            catch (Exception ex)
            {
                var chat = update.Message?.Chat;
                if (chat != null)
                {
                    await botClient.SendMessage(chat, $"Произошла ошибка: {ex.Message}", cancellationToken);
                }
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[ОШИБКА] {exception.Message}");
            Console.WriteLine(exception.StackTrace);
            return Task.CompletedTask;
        }
    }
}