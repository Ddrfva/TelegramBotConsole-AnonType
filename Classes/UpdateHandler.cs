using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Core.Services;
using Core.Entities;
using TelegramBot_29.TelegramBot.Scenarios;
using TelegramBot_29.TelegramBot.Dto;
using TelegramBot_29.Helpers;
using System.Linq;

namespace TelegramBot_29.TelegramBot
{
    public class UpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _reportService;
        private readonly IToDoListService _listService;
        private readonly IScenarioContextRepository _contextRepository;
        private readonly IEnumerable<IScenario> _scenarios;
        private readonly Dictionary<long, ToDoUser> _users = new();
        private const int _pageSize = 5;

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
                if (update.CallbackQuery is { } callbackQuery)
                {
                    await HandleCallbackQueryAsync(botClient, callbackQuery, cancellationToken);
                    return;
                }

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

                if (messageText == "/show")
                {
                    await HandleShowCommand(botClient, chatId, user, cancellationToken);
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

        private async Task HandleShowCommand(ITelegramBotClient botClient, long chatId, ToDoUser user, CancellationToken cancellationToken)
        {
            var lists = await _listService.GetUserLists(user.UserId, cancellationToken);

            var buttons = new List<InlineKeyboardButton[]>
            {
                new[] { InlineKeyboardButton.WithCallbackData("📌Без списка", new PagedListCallbackDto("show", null, 0).ToString()) }
            };

            buttons.AddRange(lists.Select(list =>
                new[] { InlineKeyboardButton.WithCallbackData(list.Name, new PagedListCallbackDto("show", list.Id, 0).ToString()) }
            ));

            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist") });

            var keyboard = new InlineKeyboardMarkup(buttons);

            await botClient.SendMessage(chatId, "Выберите список:", replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        private InlineKeyboardMarkup BuildPagedButtons(
            IReadOnlyList<KeyValuePair<string, string>> callbackData,
            PagedListCallbackDto listDto)
        {
            var totalPages = (int)Math.Ceiling((double)callbackData.Count / _pageSize);
            var currentPageTasks = callbackData.GetBatchByNumber(_pageSize, listDto.Page).ToList();

            var buttons = new List<InlineKeyboardButton[]>();

            foreach (var task in currentPageTasks)
            {
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(task.Key, task.Value) });
            }

            var navButtons = new List<InlineKeyboardButton>();

            if (listDto.Page > 0)
            {
                navButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️",
                    new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, listDto.Page - 1).ToString()));
            }

            if (listDto.Page < totalPages - 1)
            {
                navButtons.Add(InlineKeyboardButton.WithCallbackData("➡️",
                    new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, listDto.Page + 1).ToString()));
            }

            if (navButtons.Any())
            {
                buttons.Add(navButtons.ToArray());
            }

            return new InlineKeyboardMarkup(buttons);
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var telegramUserId = callbackQuery.From.Id;

            if (!_users.TryGetValue(telegramUserId, out var user))
            {
                await botClient.AnswerCallbackQuery(callbackQuery.Id, "Сначала зарегистрируйтесь через /start.", cancellationToken: cancellationToken);
                return;
            }

            var data = callbackQuery.Data;
            Console.WriteLine($"[DEBUG] CallbackQuery data: {data}");

            if (data.StartsWith("selectlist|"))
            {
                var listDto = ToDoListCallbackDto.FromString(data);
                var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);

                if (context != null && context.CurrentScenario == ScenarioType.AddTask)
                {
                    if (!context.Data.ContainsKey("User") || !context.Data.ContainsKey("TaskName") || !context.Data.ContainsKey("Deadline"))
                    {
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "Ошибка: данные сценария утеряны. Начните заново.", cancellationToken: cancellationToken);
                        await _contextRepository.ResetContext(telegramUserId, cancellationToken);
                        return;
                    }

                    var userObj = (ToDoUser)context.Data["User"];
                    var taskName = (string)context.Data["TaskName"];
                    var deadline = (DateTime)context.Data["Deadline"];

                    ToDoList? selectedList = null;
                    if (listDto.ToDoListId.HasValue)
                    {
                        selectedList = await _listService.Get(listDto.ToDoListId.Value, cancellationToken);
                        if (selectedList == null)
                        {
                            await botClient.AnswerCallbackQuery(callbackQuery.Id, "Список не найден.", cancellationToken: cancellationToken);
                            return;
                        }
                    }

                    try
                    {
                        var newTask = await _todoService.Add(userObj, taskName, deadline, selectedList, cancellationToken);
                        await botClient.SendMessage(chatId,
                            $"Задача \"{taskName}\" добавлена. Дедлайн: {deadline:dd.MM.yyyy}. Id: `{newTask.Id}`" +
                            (selectedList != null ? $"\nСписок: {selectedList.Name}" : ""),
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                            cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chatId, $"Ошибка: {ex.Message}", cancellationToken: cancellationToken);
                    }

                    await _contextRepository.ResetContext(telegramUserId, cancellationToken);
                    await botClient.SendMessage(chatId, "Сценарий завершён.", replyMarkup: GetMainKeyboard(), cancellationToken: cancellationToken);
                }
                else
                {
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "Активный сценарий не найден.", cancellationToken: cancellationToken);
                }

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (data == "addlist")
            {
                var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
                if (context == null || context.CurrentScenario != ScenarioType.AddList)
                {
                    var newContext = new ScenarioContext(ScenarioType.AddList);
                    newContext.Data["User"] = user;
                    await _contextRepository.SetContext(telegramUserId, newContext, cancellationToken);
                    await ProcessScenario(botClient, newContext, callbackQuery.Message!, cancellationToken);
                }
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (data == "deletelist")
            {
                var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
                if (context == null || context.CurrentScenario != ScenarioType.DeleteList)
                {
                    var newContext = new ScenarioContext(ScenarioType.DeleteList);
                    newContext.Data["User"] = user;
                    await _contextRepository.SetContext(telegramUserId, newContext, cancellationToken);
                    await ProcessScenario(botClient, newContext, callbackQuery.Message!, cancellationToken);
                }
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (data.StartsWith("show|") || data == "show")
            {
                var listDto = PagedListCallbackDto.FromString(data);
                var tasks = await _todoService.GetByUserIdAndList(user.UserId, listDto.ToDoListId, cancellationToken);

                var listName = listDto.ToDoListId.HasValue
                    ? (await _listService.Get(listDto.ToDoListId.Value, cancellationToken))?.Name ?? "Список"
                    : "Без списка";

                var taskButtons = tasks.Select(task =>
                    new KeyValuePair<string, string>(
                        $"{task.Name} - {task.Deadline:dd.MM.yyyy}",
                        new ToDoItemCallbackDto("showtask", task.Id).ToString()
                    )
                ).ToList();

                var keyboard = BuildPagedButtons(taskButtons, listDto);

                var extraButtons = new List<InlineKeyboardButton[]>
                {
                    new[] { InlineKeyboardButton.WithCallbackData("☑️Посмотреть выполненные",
                        new PagedListCallbackDto("show_completed", listDto.ToDoListId, 0).ToString()) }
                };

                var allButtons = keyboard.InlineKeyboard.Concat(extraButtons).ToArray();
                var finalKeyboard = new InlineKeyboardMarkup(allButtons);

                await botClient.EditMessageText(
                    chatId: chatId,
                    messageId: callbackQuery.Message.MessageId,
                    text: $"Задачи в списке \"{listName}\":",
                    replyMarkup: finalKeyboard,
                    cancellationToken: cancellationToken);

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (data.StartsWith("showtask|"))
            {
                var taskDto = ToDoItemCallbackDto.FromString(data);
                var task = await _todoService.Get(taskDto.ToDoItemId, cancellationToken);

                if (task == null)
                {
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "Задача не найдена.", cancellationToken: cancellationToken);
                    return;
                }

                var taskInfo = $"📌 {task.Name}\n" +
                               $"Дедлайн: {task.Deadline:dd.MM.yyyy}\n" +
                               $"Статус: {(task.State == ToDoItemState.Active ? "Активна" : "Выполнена")}\n" +
                               $"Id: `{task.Id}`";

                var buttons = new List<InlineKeyboardButton[]>
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("✅Выполнить",
                            new ToDoItemCallbackDto("completetask", task.Id).ToString()),
                        InlineKeyboardButton.WithCallbackData("❌Удалить",
                            new ToDoItemCallbackDto("deletetask", task.Id).ToString())
                    }
                };

                var keyboard = new InlineKeyboardMarkup(buttons);

                await botClient.EditMessageText(
                    chatId: chatId,
                    messageId: callbackQuery.Message.MessageId,
                    text: taskInfo,
                    replyMarkup: keyboard,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken);

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (data.StartsWith("completetask|"))
            {
                var taskDto = ToDoItemCallbackDto.FromString(data);
                await _todoService.MarkCompleted(taskDto.ToDoItemId, cancellationToken);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, "Задача выполнена!", cancellationToken: cancellationToken);
                await botClient.SendMessage(chatId, "✅ Задача выполнена!", cancellationToken: cancellationToken);
                return;
            }

            if (data.StartsWith("deletetask|"))
            {
                var taskDto = ToDoItemCallbackDto.FromString(data);
                var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
                if (context == null || context.CurrentScenario != ScenarioType.DeleteTask)
                {
                    var newContext = new ScenarioContext(ScenarioType.DeleteTask);
                    newContext.Data["TaskId"] = taskDto.ToDoItemId;
                    await _contextRepository.SetContext(telegramUserId, newContext, cancellationToken);
                    await ProcessScenario(botClient, newContext, callbackQuery.Message!, cancellationToken);
                }
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (data.StartsWith("show_completed|") || data == "show_completed")
            {
                var listDto = PagedListCallbackDto.FromString(data);
                var allTasks = await _todoService.GetByUserIdAndList(user.UserId, listDto.ToDoListId, cancellationToken);
                var completedTasks = allTasks.Where(t => t.State == ToDoItemState.Completed).ToList();

                if (!completedTasks.Any())
                {
                    await botClient.EditMessageText(
                        chatId: chatId,
                        messageId: callbackQuery.Message.MessageId,
                        text: "Задач нет.",
                        cancellationToken: cancellationToken);
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                    return;
                }

                var listName = listDto.ToDoListId.HasValue
                    ? (await _listService.Get(listDto.ToDoListId.Value, cancellationToken))?.Name ?? "Список"
                    : "Без списка";

                var taskButtons = completedTasks.Select(task =>
                    new KeyValuePair<string, string>(
                        $"☑️ {task.Name} - {task.Deadline:dd.MM.yyyy}",
                        new ToDoItemCallbackDto("showtask", task.Id).ToString()
                    )
                ).ToList();

                var keyboard = BuildPagedButtons(taskButtons, listDto);

                await botClient.EditMessageText(
                    chatId: chatId,
                    messageId: callbackQuery.Message.MessageId,
                    text: $"Выполненные задачи в списке \"{listName}\":",
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken);

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (data == "yes")
            {
                var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
                if (context != null)
                {
                    if (context.CurrentScenario == ScenarioType.DeleteList && context.Data.TryGetValue("ListToDelete", out var listObj))
                    {
                        var list = (ToDoList)listObj;
                        var tasks = await _todoService.GetByUserIdAndList(user.UserId, list.Id, cancellationToken);
                        foreach (var task in tasks)
                        {
                            await _todoService.Delete(task.Id, cancellationToken);
                        }
                        await _listService.Delete(list.Id, cancellationToken);
                        await botClient.SendMessage(chatId, $"Список \"{list.Name}\" и все его задачи удалены.", cancellationToken: cancellationToken);
                        await _contextRepository.ResetContext(telegramUserId, cancellationToken);
                    }
                    else if (context.CurrentScenario == ScenarioType.DeleteTask && context.Data.TryGetValue("Task", out var taskObj))
                    {
                        var task = (ToDoItem)taskObj;
                        await _todoService.Delete(task.Id, cancellationToken);
                        await botClient.SendMessage(chatId, $"Задача \"{task.Name}\" удалена.", cancellationToken: cancellationToken);
                        await _contextRepository.ResetContext(telegramUserId, cancellationToken);
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Ошибка: данные для удаления не найдены.", cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    await botClient.SendMessage(chatId, "Ошибка: контекст не найден.", cancellationToken: cancellationToken);
                }
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (data == "no")
            {
                var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
                if (context != null)
                {
                    await _contextRepository.ResetContext(telegramUserId, cancellationToken);
                }
                await botClient.SendMessage(chatId, "Удаление отменено.", cancellationToken: cancellationToken);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            await botClient.AnswerCallbackQuery(callbackQuery.Id, "Неизвестное действие.", cancellationToken: cancellationToken);
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
                new KeyboardButton[] { "/addtask", "/show" },
                new KeyboardButton[] { "/report", "/exit" }
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