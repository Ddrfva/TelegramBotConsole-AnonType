using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Core.Services;
<<<<<<< HEAD
using Core.Entities;
using TelegramBot_27.TelegramBot.Scenarios;
using System.Linq;

namespace TelegramBot_27.TelegramBot
=======
using Core.DataAccess;
using Core.Entities;
using Infrastructure.DataAccess;
using System.Linq;

namespace TelegramBot_26.Classes
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
{
    public class UpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _reportService;
<<<<<<< HEAD
        private readonly IScenarioContextRepository _contextRepository;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly Dictionary<long, ToDoUser> _users = new();

        public UpdateHandler(
            IUserService userService,
            IToDoService todoService,
            IToDoReportService reportService,
            IScenarioContextRepository contextRepository,
            IEnumerable<IScenario> scenarios)
=======
        private readonly Dictionary<long, ToDoUser> _users = new();
        private readonly Dictionary<long, bool> _waitingForTaskDescription = new();

        public UpdateHandler(IUserService userService, IToDoService todoService, IToDoReportService reportService)
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
        {
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
<<<<<<< HEAD
            _contextRepository = contextRepository;
            _scenarios = scenarios;
=======
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
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
<<<<<<< HEAD
                    user = await _userService.GetUser(telegramUserId, cancellationToken);
                    if (user == null)
                    {
                        user = await _userService.RegisterUser(telegramUserId, telegramUserName, cancellationToken);
                        _users[telegramUserId] = user;
                    }
                    else
                    {
                        _users[telegramUserId] = user;
                    }
                }

                if (messageText == "/cancel")
                {
                    await _contextRepository.ResetContext(telegramUserId, cancellationToken);
                    await botClient.SendMessage(chatId, "Сценарий отменён.", replyMarkup: GetMainKeyboard(), cancellationToken: cancellationToken);
                    return;
                }

                var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
                if (context != null && context.CurrentScenario != ScenarioType.None)
                {
                    await ProcessScenario(botClient, context, message, cancellationToken);
                    return;
                }

                if (messageText.StartsWith("/completetask"))
=======
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
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
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

<<<<<<< HEAD
                var replyKeyboard = GetMainKeyboard();

=======
                // Точные команды (без аргументов)
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
                switch (messageText)
                {
                    case "/start":
                        await botClient.SendMessage(chatId, $"Добро пожаловать, {user.TelegramUserName}!", replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                        break;

                    case "/addtask":
<<<<<<< HEAD
                        var newContext = new ScenarioContext(ScenarioType.AddTask);
                        newContext.Data["User"] = user;
                        await _contextRepository.SetContext(telegramUserId, newContext, cancellationToken);
                        await ProcessScenario(botClient, newContext, message, cancellationToken);
=======
                        _waitingForTaskDescription[chatId] = true;
                        await botClient.SendMessage(chatId, "Введите описание задачи:", cancellationToken: cancellationToken);
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
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
<<<<<<< HEAD
                                response += $"{task.Name} - Дедлайн: {task.Deadline:dd.MM.yyyy} - `{task.Id}`\n";
=======
                                response += $"{task.Name} - `{task.Id}`\n";
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
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
<<<<<<< HEAD
                                response += $"{task.State} - {task.Name} - Дедлайн: {task.Deadline:dd.MM.yyyy} - `{task.Id}`\n";
=======
                                response += $"{task.State} - {task.Name} - `{task.Id}`\n";
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
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

<<<<<<< HEAD
        private async Task ProcessScenario(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);
            if (scenario == null)
                throw new Exception($"Сценарий для {context.CurrentScenario} не найден");

            var result = await scenario.HandleMessageAsync(botClient, context, message, ct);
            if (result == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(message.From!.Id, ct);
                await botClient.SendMessage(message.Chat.Id, "Сценарий завершён.", replyMarkup: GetMainKeyboard(), cancellationToken: ct);
            }
            else
            {
                await _contextRepository.SetContext(message.From!.Id, context, ct);
            }
        }

        private IScenario? GetScenario(ScenarioType type) => _scenarios.FirstOrDefault(s => s.CanHandle(type));

        private ReplyKeyboardMarkup GetMainKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "/addtask", "/showtasks" },
                new KeyboardButton[] { "/showalltasks", "/report" },
                new KeyboardButton[] { "/completetask", "/exit" }
            })
            { ResizeKeyboard = true };
=======
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
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            await Task.CompletedTask;
        }
    }
}