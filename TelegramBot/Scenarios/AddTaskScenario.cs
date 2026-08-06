using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Services;
using Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_29.TelegramBot.Dto;

namespace TelegramBot_29.TelegramBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoListService _listService;

        public AddTaskScenario(IUserService userService, IToDoService todoService, IToDoListService listService)
        {
            _userService = userService;
            _todoService = todoService;
            _listService = listService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
        {
            var chatId = message.Chat.Id;
            var userId = message.From?.Id ?? 0;

            switch (context.CurrentStep)
            {
                case null:
                    var user = (ToDoUser)context.Data["User"];
                    context.CurrentStep = "Name";
                    await bot.SendMessage(chatId, "Введите название задачи:", replyMarkup: new ReplyKeyboardMarkup(new[] { new KeyboardButton("/cancel") }) { ResizeKeyboard = true }, cancellationToken: ct);
                    return ScenarioResult.Transition;

                case "Name":
                    var taskName = message.Text?.Trim();
                    if (string.IsNullOrWhiteSpace(taskName))
                    {
                        await bot.SendMessage(chatId, "Название не может быть пустым. Попробуйте снова:", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                    context.Data["TaskName"] = taskName;
                    context.CurrentStep = "Deadline";
                    await bot.SendMessage(chatId, "Введите дату выполнения (день.месяц.год, например 31.12.2026):", cancellationToken: ct);
                    return ScenarioResult.Transition;

                case "Deadline":
                    if (!DateTime.TryParseExact(message.Text?.Trim(), "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime deadline))
                    {
                        await bot.SendMessage(chatId, "Неверный формат. Введите дату в формате день.месяц.год (например, 31.12.2026):", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                    var userObj = (ToDoUser)context.Data["User"];
                    var taskNameObj = (string)context.Data["TaskName"];
                    context.Data["Deadline"] = deadline;

                    var lists = await _listService.GetUserLists(userObj.UserId, ct);
                    var buttons = lists.Select(list =>
                        InlineKeyboardButton.WithCallbackData(list.Name, new ToDoListCallbackDto("selectlist", list.Id).ToString())
                    ).ToList();

                    buttons.Insert(0, InlineKeyboardButton.WithCallbackData("📌Без списка", new ToDoListCallbackDto("selectlist", null).ToString()));

                    var keyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }));

                    await bot.SendMessage(chatId, "Выберите список для задачи:", replyMarkup: keyboard, cancellationToken: ct);
                    context.CurrentStep = "SelectList";
                    return ScenarioResult.Transition;

                case "SelectList":
                    return ScenarioResult.Completed;

                default:
                    await bot.SendMessage(chatId, "Неизвестный шаг. Начните заново с /addtask.", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }
    }
}