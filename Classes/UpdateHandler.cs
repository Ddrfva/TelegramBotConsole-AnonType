using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Core.Services;
using Core.Entities;
using TelegramBot_27_2.Scenarios;
using System.Linq;

namespace TelegramBot_27_2.Classes
{
    public class UpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _reportService;
        private readonly IScenarioContextRepository _contextRepository;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly Dictionary<long, ToDoUser> _users = new();

        public UpdateHandler(
            IUserService userService,
            IToDoService todoService,
            IToDoReportService reportService,
            IScenarioContextRepository contextRepository,
            IEnumerable<IScenario> scenarios)
        {
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
            _contextRepository = contextRepository;
            _scenarios = scenarios;
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

                var replyKeyboard = GetMainKeyboard();

                switch (messageText)
                {
                    case "/start":
                        await botClient.SendMessage(chatId, $"Добро пожаловать, {user.TelegramUserName}!", replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                        break;

                    case "/addtask":
                        var newContext = new ScenarioContext(ScenarioType.AddTask);
                        newContext.Data["User"] = user;
                        await _contextRepository.SetContext(telegramUserId, newContext, cancellationToken);
                        await ProcessScenario(botClient, newContext, message, cancellationToken);
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
                                response += $"{task.Name} - Дедлайн: {task.Deadline:dd.MM.yyyy} - `{task.Id}`\n";
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
                                response += $"{task.State} - {task.Name} - Дедлайн: {task.Deadline:dd.MM.yyyy} - `{task.Id}`\n";
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
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            await Task.CompletedTask;
        }
    }
}