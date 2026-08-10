using System;
using System.Threading.Tasks;
using Core.Services;
using Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot_29.TelegramBot.Scenarios
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
            var userId = message.From?.Id ?? 0;

            switch (context.CurrentStep)
            {
                case null:
                    var taskId = (Guid)context.Data["TaskId"];
                    var task = await _todoService.Get(taskId, ct);
                    if (task == null)
                    {
                        await bot.SendMessage(chatId, "Задача не найдена.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["Task"] = task;

                    await bot.SendMessage(chatId, $"Подтверждаете удаление задачи \"{task.Name}\"?",
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                                InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                            }
                        }),
                        cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    return ScenarioResult.Transition;

                case "Approve":
                    return ScenarioResult.Completed;

                default:
                    await bot.SendMessage(chatId, "Неизвестный шаг. Начните заново.", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }
    }
}