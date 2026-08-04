using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Core.Services;
using Core.Entities;
using TelegramBot_28.TelegramBot.Scenarios;
using TelegramBot_28.TelegramBot.Dto;
using System.Linq;

namespace TelegramBot_28.TelegramBot
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
                new[] { InlineKeyboardButton.WithCallbackData("📌Без списка", new ToDoListCallbackDto("show", null).ToString()) }
            };

            buttons.AddRange(lists.Select(list =>
                new[] { InlineKeyboardButton.WithCallbackData(list.Name, new ToDoListCallbackDto("show", list.Id).ToString()) }
            ));

            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist") });

            var keyboard = new InlineKeyboardMarkup(buttons);

            await botClient.SendMessage(chatId, "Выберите список:", replyMarkup: keyboard, cancellationToken: cancellationToken);
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

            if (data == "deletelist")
            {
                Console.WriteLine($"[DEBUG] Запуск сценария удаления");
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

            if (data.StartsWith("delete_"))
            {
                var idString = data.Substring(7);
                Console.WriteLine($"[DEBUG] delete_ idString: {idString}");

                if (!Guid.TryParse(idString, out Guid listId))
                {
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "Неверный идентификатор списка.", cancellationToken: cancellationToken);
                    return;
                }

                Console.WriteLine($"[DEBUG] Найден Id списка: {listId}");

                var list = await _listService.Get(listId, cancellationToken);
                if (list == null)
                {
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "Список не найден.", cancellationToken: cancellationToken);
                    return;
                }

                var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
                if (context == null)
                {
                    context = new ScenarioContext(ScenarioType.DeleteList);
                    context.Data["User"] = user;
                    await _contextRepository.SetContext(telegramUserId, context, cancellationToken);
                }
                context.Data["ListToDelete"] = list;

                await botClient.SendMessage(chatId, $"Подтверждаете удаление списка \"{list.Name}\" и всех его задач?",
                    replyMarkup: new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                            InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                        }
                    }),
                    cancellationToken: cancellationToken);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (data == "yes")
            {
                var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
                if (context != null && context.Data.TryGetValue("ListToDelete", out var listObj))
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
                else
                {
                    await botClient.SendMessage(chatId, "Ошибка: список не найден в контексте.", cancellationToken: cancellationToken);
                }
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (data == "no")
            {
                await botClient.SendMessage(chatId, "Удаление отменено.", cancellationToken: cancellationToken);
                await _contextRepository.ResetContext(telegramUserId, cancellationToken);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            var callbackDto = CallbackDto.FromString(data);

            if (callbackDto.Action == "selectlist")
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

            if (callbackDto.Action == "show")
            {
                var listDto = ToDoListCallbackDto.FromString(data);
                var tasks = await _todoService.GetByUserIdAndList(user.UserId, listDto.ToDoListId, cancellationToken);

                if (!tasks.Any())
                {
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "В этом списке нет задач.", cancellationToken: cancellationToken);
                    await botClient.SendMessage(chatId, "В этом списке нет задач.", cancellationToken: cancellationToken);
                    return;
                }

                var listName = listDto.ToDoListId.HasValue
                    ? (await _listService.Get(listDto.ToDoListId.Value, cancellationToken))?.Name ?? "Список"
                    : "Без списка";

                var response = $"Задачи в списке \"{listName}\":\n";
                foreach (var task in tasks)
                    response += $"{task.Name} - Дедлайн: {task.Deadline:dd.MM.yyyy} - `{task.Id}`\n";

                await botClient.SendMessage(chatId, response, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, cancellationToken: cancellationToken);
                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                return;
            }

            if (callbackDto.Action == "addlist")
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