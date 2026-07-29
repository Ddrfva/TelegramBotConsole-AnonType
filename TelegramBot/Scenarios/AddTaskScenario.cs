using System;
<<<<<<< HEAD
=======
using System.Collections.Generic;
using System.Linq;
using System.Text;
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
using System.Threading.Tasks;
using Core.Services;
using Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot_27.TelegramBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;

        public AddTaskScenario(IUserService userService, IToDoService todoService)
        {
            _userService = userService;
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
        {
            var chatId = message.Chat.Id;
            var userId = message.From?.Id ?? 0;

            switch (context.CurrentStep)
            {
                case null:
<<<<<<< HEAD
                    var user = (ToDoUser)context.Data["User"];
=======

                    var user = await _userService.GetUser(userId, ct);
                    if (user == null)
                    {
                        await bot.SendMessage(chatId, "Сначала зарегистрируйтесь через /start", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    context.Data["User"] = user;
>>>>>>> 612ae305cfc875d783b7d13ecc54187068b59989
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

                    try
                    {
                        var newTask = await _todoService.Add(userObj, taskNameObj, deadline, ct);
                        await bot.SendMessage(chatId, $"Задача \"{taskNameObj}\" добавлена. Дедлайн: {deadline:dd.MM.yyyy}. Id: `{newTask.Id}`", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await bot.SendMessage(chatId, $"Ошибка: {ex.Message}", cancellationToken: ct);
                    }

                    return ScenarioResult.Completed;

                default:
                    await bot.SendMessage(chatId, "Неизвестный шаг. Начните заново с /addtask.", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }
    }
}