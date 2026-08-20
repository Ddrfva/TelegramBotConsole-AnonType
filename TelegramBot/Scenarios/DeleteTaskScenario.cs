using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Services;
using Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_31.Scenarios;

namespace TelegramBot_31.Scenarios
{
    public class DeleteTaskScenario : IScenario
    {
        private readonly IToDoService _todoService;

        public DeleteTaskScenario(IToDoService todoService)
        {
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
        {
            var chatId = message.Chat.Id;

            if (!context.Data.TryGetValue("User", out var userObj) || userObj is not ToDoUser user)
            {
                await bot.SendMessage(chatId, "Сначала зарегистрируйтесь через /start", cancellationToken: ct);
                return ScenarioResult.Completed;
            }

            switch (context.CurrentStep)
            {
                case null:
                    var tasks = await _todoService.GetActiveByUserId(user.Id, ct);
                    if (!tasks.Any())
                    {
                        await bot.SendMessage(chatId, "У вас нет активных задач для удаления.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    var buttons = tasks.Select(task =>
                    {
                        var callbackData = $"deletetask_{task.Id}";
                        var displayName = task.Name.Length > 30 ? task.Name.Substring(0, 30) + "..." : task.Name;
                        return InlineKeyboardButton.WithCallbackData($"🗑️ {displayName}", callbackData);
                    }).ToList();

                    var rows = new List<InlineKeyboardButton[]>();
                    for (int i = 0; i < buttons.Count; i += 2)
                    {
                        var row = buttons.Skip(i).Take(2).ToArray();
                        rows.Add(row);
                    }

                    var keyboard = new InlineKeyboardMarkup(rows);

                    await bot.SendMessage(chatId, "Выберите задачу для удаления:",
                        replyMarkup: keyboard,
                        cancellationToken: ct);

                    context.CurrentStep = "SelectTask";
                    return ScenarioResult.Transition;

                case "SelectTask":
                    return ScenarioResult.Transition;

                default:
                    await bot.SendMessage(chatId, "Неизвестный шаг. Начните заново.", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }
    }
}