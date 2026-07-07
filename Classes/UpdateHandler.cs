using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Core.Services;
using Core.DataAccess;
using Core.Entities;
using Infrastructure.DataAccess;
using System.Linq;

namespace TelegramBot_25.Classes
{
    public class UpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _reportService;
        private readonly Dictionary<long, ToDoUser> _users = new();
        private readonly Dictionary<long, bool> _waitingForTaskDescription = new();

        public UpdateHandler()
        {
            var userRepository = new InMemoryUserRepository();
            var todoRepository = new InMemoryToDoRepository();
            _userService = new UserService(userRepository);
            _todoService = new ToDoService(todoRepository, userRepository, 100, 500);
            _reportService = new ToDoReportService(todoRepository);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message is not { Text: { } messageText } message)
                    return;

                var chatId = message.Chat.Id;
                var telegramUserId = message.From?.Id ?? 0;
                var telegramUserName = message.From?.Username ?? "User";

                if (!_users.TryGetValue(telegramUserId, out var user))
                {
                    user = await _userService.RegisterUser(telegramUserId, telegramUserName, cancellationToken);
                    _users[telegramUserId] = user;
                }

                var replyKeyboard = GetReplyKeyboard(user != null);

                if (_waitingForTaskDescription.TryGetValue(chatId, out bool waiting) && waiting)
                {
                    _waitingForTaskDescription[chatId] = false;

                    if (string.IsNullOrWhiteSpace(messageText))
                    {
                        await botClient.SendMessage(chatId, "Описание задачи не может быть пустым.", cancellationToken: cancellationToken);
                        return;
                    }

                    try
                    {
                        var newTask = await _todoService.Add(user, messageText, cancellationToken);
                        await botClient.SendMessage(chatId, $"Задача \"{messageText}\" добавлена. Id: `{newTask.Id}`", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chatId, $"Ошибка: {ex.Message}", cancellationToken: cancellationToken);
                    }
                    return;
                }

                if (messageText.StartsWith("/completetask "))
                {
                    var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                    {
                        await botClient.SendMessage(chatId, "Укажите Id задачи. Пример: /completetask 66b7c15b-8627-49db-aece-fe087b4b4095", cancellationToken: cancellationToken);
                        return;
                    }

                    var taskIdString = parts[1];
                    if (!Guid.TryParse(taskIdString, out Guid taskId))
                    {
                        await botClient.SendMessage(chatId, "Неверный формат Id. Id должен быть в формате GUID.", cancellationToken: cancellationToken);
                        return;
                    }

                    try
                    {
                        await _todoService.MarkCompleted(taskId, cancellationToken);
                        await botClient.SendMessage(chatId, $"Задача с Id `{taskId}` завершена.", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chatId, $"Ошибка при завершении задачи: {ex.Message}", cancellationToken: cancellationToken);
                    }
                    return;
                }

                // Точные команды (без аргументов)
                switch (messageText)
                {
                    case "/start":
                        await botClient.SendMessage(chatId, $"Добро пожаловать, {user.TelegramUserName}!", replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                        break;

                    case "/addtask":
                        _waitingForTaskDescription[chatId] = true;
                        await botClient.SendMessage(chatId, "Введите описание задачи:", cancellationToken: cancellationToken);
                        break;

                    case "/showtasks":
                        var activeTasks = await _todoService.GetActiveByUserId(user.UserId, cancellationToken);
                        if (!activeTasks.Any())
                        {
                            await botClient.SendMessage(chatId, "Нет активных задач.", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            var response = "Активные задачи:\n";
                            foreach (var task in activeTasks)
                                response += $"{task.Name} - `{task.Id}`\n";
                            await botClient.SendMessage(chatId, response, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, cancellationToken: cancellationToken);
                        }
                        break;

                    case "/showalltasks":
                        var allTasks = await _todoService.GetAllByUserId(user.UserId, cancellationToken);
                        if (!allTasks.Any())
                        {
                            await botClient.SendMessage(chatId, "Нет задач.", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            var response = "Все задачи:\n";
                            foreach (var task in allTasks)
                                response += $"{task.State} - {task.Name} - `{task.Id}`\n";
                            await botClient.SendMessage(chatId, response, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, cancellationToken: cancellationToken);
                        }
                        break;

                    case "/report":
                        var stats = await _reportService.GetUserStats(user.UserId, cancellationToken);
                        await botClient.SendMessage(chatId, $"Всего: {stats.total}, Активных: {stats.active}, Завершённых: {stats.completed}", cancellationToken: cancellationToken);
                        break;

                    default:
                        await botClient.SendMessage(chatId, "Используйте команды из меню.", replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private ReplyKeyboardMarkup GetReplyKeyboard(bool isRegistered)
        {
            var buttons = new List<List<KeyboardButton>>();

            if (!isRegistered)
            {
                buttons.Add([new KeyboardButton("/start")]);
            }
            else
            {
                buttons.Add([new KeyboardButton("/showtasks"), new KeyboardButton("/showalltasks")]);
                buttons.Add([new KeyboardButton("/report")]);
            }

            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            await Task.CompletedTask;
        }
    }
}