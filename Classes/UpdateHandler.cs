using Core.Entities;
using Core.Services;
using System.Collections.Concurrent;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_31.Scenarios;
using System.Collections.Concurrent;

namespace TelegramBot_31.Classes
{
    public class UpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _reportService;
        private readonly IToDoListService _listService;
        private readonly IScenarioContextRepository _contextRepository;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly ConcurrentDictionary<long, ToDoUser> _users = new();

        public UpdateHandler(
            IUserService userService,
            IToDoService todoService,
            IToDoReportService reportService,
            IToDoListService listService,
            IScenarioContextRepository contextRepository,
            IEnumerable<IScenario> scenarios)
        {
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
            _listService = listService;
            _contextRepository = contextRepository;
            _scenarios = scenarios;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                {
                    await HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
                    return;
                }

                if (update.Message is not { Text: { } messageText } message)
                    return;

                var chatId = message.Chat.Id;
                var telegramUserId = message.From?.Id ?? 0;
                var telegramUserName = message.From?.Username ?? "User";

                if (!_users.TryGetValue(telegramUserId, out var user))
                {
                    user = await _userService.GetUserByTelegramUserId(telegramUserId, cancellationToken);
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
                    await botClient.SendMessage(
                        chatId,
                        "Сценарий отменён.",
                        replyMarkup: GetMainKeyboard(),
                        cancellationToken: cancellationToken);
                    return;
                }

                var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
                if (context != null && context.CurrentScenario != ScenarioType.None)
                {
                    await ProcessScenario(botClient, context, message, cancellationToken);
                    return;
                }

                var replyKeyboard = GetMainKeyboard();

                switch (messageText)
                {
                    case "/start":
                        await botClient.SendMessage(
                            chatId,
                            $"Добро пожаловать, {user.TelegramUserName}! 🌿\n" +
                            "Я помогу тебе ухаживать за растениями.\n" +
                            "Используй команды из меню.",
                            replyMarkup: replyKeyboard,
                            cancellationToken: cancellationToken);
                        break;

                    case "/addtask":
                        var newContext = new ScenarioContext(ScenarioType.AddTask);
                        newContext.Data["User"] = user;
                        await _contextRepository.SetContext(telegramUserId, newContext, cancellationToken);
                        await ProcessScenario(botClient, newContext, message, cancellationToken);
                        break;

                    case "/addlist":
                        var listContext = new ScenarioContext(ScenarioType.AddList);
                        listContext.Data["User"] = user;
                        await _contextRepository.SetContext(telegramUserId, listContext, cancellationToken);
                        await ProcessScenario(botClient, listContext, message, cancellationToken);
                        break;

                    case "/show":
                        await HandleShowCommand(botClient, chatId, user.Id, cancellationToken);
                        break;

                    case "/report":
                        var stats = await _reportService.GetUserStats(user.Id, cancellationToken);
                        await botClient.SendMessage(
                            chatId,
                            $"📊 Статистика:\n" +
                            $"Всего: {stats.total}\n" +
                            $"Активных: {stats.active}\n" +
                            $"Завершённых: {stats.completed}",
                            cancellationToken: cancellationToken);
                        break;

                    case "/exit":
                        await botClient.SendMessage(
                            chatId,
                            "До свидания! 👋",
                            cancellationToken: cancellationToken);
                        break;

                    default:
                        await botClient.SendMessage(
                            chatId,
                            "Используйте команды из меню.",
                            replyMarkup: replyKeyboard,
                            cancellationToken: cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в HandleUpdateAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                try
                {
                    if (update.Message?.Chat.Id != null)
                    {
                        await botClient.SendMessage(
                            update.Message.Chat.Id,
                            "Произошла ошибка. Попробуйте позже.",
                            cancellationToken: cancellationToken);
                    }
                }
                catch { }
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken ct)
        {
            var chatId = callbackQuery.Message?.Chat.Id ?? 0;
            var telegramUserId = callbackQuery.From?.Id ?? 0;

            var data = callbackQuery.Data;
            if (string.IsNullOrEmpty(data))
                return;

            if (data.StartsWith("complete_"))
            {
                var taskIdStr = data.Replace("complete_", "");
                if (Guid.TryParse(taskIdStr, out var taskId))
                {
                    await _todoService.MarkCompleted(taskId, ct);
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "✅ Задача завершена!", cancellationToken: ct);

                    if (callbackQuery.Message != null)
                    {
                        await botClient.EditMessageText(
                            chatId,
                            callbackQuery.Message.MessageId,
                            "✅ Задача завершена!",
                            cancellationToken: ct);
                    }
                }
            }
            else if (data.StartsWith("deletetask_"))
            {
                var taskIdStr = data.Replace("deletetask_", "");
                if (Guid.TryParse(taskIdStr, out var taskId))
                {
                    await _todoService.Delete(taskId, ct);
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "🗑️ Задача удалена!", cancellationToken: ct);

                    if (callbackQuery.Message != null)
                    {
                        await botClient.EditMessageText(
                            chatId,
                            callbackQuery.Message.MessageId,
                            "🗑️ Задача удалена.",
                            cancellationToken: ct);
                    }

                    await _contextRepository.ResetContext(telegramUserId, ct);
                }
            }
            else if (data.StartsWith("list_"))
            {
                var listIdStr = data.Replace("list_", "");
                if (Guid.TryParse(listIdStr, out var listId))
                {
                    var context = await _contextRepository.GetContext(telegramUserId, ct);
                    if (context != null)
                    {
                        context.Data["SelectedListId"] = listId;
                        await _contextRepository.SetContext(telegramUserId, context, ct);

                        if (context.Data.TryGetValue("User", out var userObj) && userObj is ToDoUser user)
                        {
                            var name = context.Data["PlantName"].ToString();
                            var species = context.Data.ContainsKey("Species") ? context.Data["Species"]?.ToString() : null;

                            var item = await _todoService.Add(user, name, listId, ct);

                            await botClient.SendMessage(
                                chatId,
                                $"✅ Растение \"{name}\" добавлено в список! 🌱",
                                cancellationToken: ct);
                            await _contextRepository.ResetContext(telegramUserId, ct);
                        }
                    }
                }
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
            }
            else if (data == "list_null")
            {
                var context = await _contextRepository.GetContext(telegramUserId, ct);
                if (context != null)
                {
                    if (context.Data.TryGetValue("User", out var userObj) && userObj is ToDoUser user)
                    {
                        var name = context.Data["PlantName"].ToString();
                        var species = context.Data.ContainsKey("Species") ? context.Data["Species"]?.ToString() : null;

                        var item = await _todoService.Add(user, name, null, ct);

                        await botClient.SendMessage(
                            chatId,
                            $"✅ Растение \"{name}\" добавлено! 🌱",
                            cancellationToken: ct);
                        await _contextRepository.ResetContext(telegramUserId, ct);
                    }
                }
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
            }
            else if (data.StartsWith("deletelist_"))
            {
                var listIdStr = data.Replace("deletelist_", "");
                if (Guid.TryParse(listIdStr, out var listId))
                {
                    await _listService.Delete(listId, ct);
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "🗑️ Список удален!", cancellationToken: ct);

                    if (callbackQuery.Message != null)
                    {
                        await botClient.EditMessageText(
                            chatId,
                            callbackQuery.Message.MessageId,
                            "🗑️ Список удален.",
                            cancellationToken: ct);
                    }

                    await _contextRepository.ResetContext(telegramUserId, ct);
                }
            }

            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
        }

        private async Task HandleShowCommand(ITelegramBotClient botClient, long chatId, Guid userId, CancellationToken ct)
        {
            var lists = await _listService.GetUserLists(userId, ct);

            if (!lists.Any())
            {
                await botClient.SendMessage(
                    chatId,
                    "📋 У вас нет списков. Создайте первый список через /addlist",
                    cancellationToken: ct);
                return;
            }

            var response = "📋 Ваши задачи:\n\n";

            foreach (var list in lists)
            {
                response += $"📁 *{list.Name}*\n";

                var allTasks = await _todoService.GetAllByUserId(userId, ct);
                var listTasks = allTasks.Where(t => t.ListId == list.Id).ToList();
                var activeTasks = listTasks.Where(t => t.State == ToDoItemState.Active).ToList();
                var completedTasks = listTasks.Where(t => t.State == ToDoItemState.Completed).ToList();

                if (!activeTasks.Any() && !completedTasks.Any())
                {
                    response += "  ✨ Нет задач в этом списке\n";
                }
                else
                {
                    if (activeTasks.Any())
                    {
                        response += "  🔹 Активные:\n";
                        foreach (var task in activeTasks)
                        {
                            response += $"    🌱 {task.Name}";
                            if (!string.IsNullOrEmpty(task.Species))
                                response += $" ({task.Species})";
                            if (task.WateringFrequencyDays.HasValue)
                                response += $" 💧 каждые {task.WateringFrequencyDays} дней";
                            response += $"\n";
                        }
                    }

                    if (completedTasks.Any())
                    {
                        response += "  ✅ Завершённые:\n";
                        foreach (var task in completedTasks.Take(5))
                        {
                            response += $"    ✓ {task.Name}\n";
                        }
                        if (completedTasks.Count > 5)
                            response += $"    ... и ещё {completedTasks.Count - 5} задач\n";
                    }
                }
                response += "\n";
            }

            await botClient.SendMessage(
                chatId,
                response,
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }

        private async Task ProcessScenario(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);
            if (scenario == null)
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    "Ошибка: сценарий не найден",
                    cancellationToken: ct);
                return;
            }

            var result = await scenario.HandleMessageAsync(botClient, context, message, ct);

            if (result == ScenarioResult.Completed)
            {
                await _contextRepository.ResetContext(message.From!.Id, ct);
                await botClient.SendMessage(
                    message.Chat.Id,
                    "✅ Сценарий завершён.",
                    replyMarkup: GetMainKeyboard(),
                    cancellationToken: ct);
            }
            else
            {
                await _contextRepository.SetContext(message.From!.Id, context, ct);
            }
        }

        private IScenario? GetScenario(ScenarioType type) =>
            _scenarios.FirstOrDefault(s => s.CanHandle(type));

        private ReplyKeyboardMarkup GetMainKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "/addtask", "/addlist" },
                new KeyboardButton[] { "/show", "/report" },
                new KeyboardButton[] { "/cancel", "/exit" }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = false
            };
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            Console.WriteLine($"Stack trace: {exception.StackTrace}");
            await Task.CompletedTask;
        }
    }
}