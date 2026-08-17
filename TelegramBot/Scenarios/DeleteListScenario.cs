using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Services;
using Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot_27_2.Scenarios;

namespace TelegramBot_27_2.Scenarios
{
    public class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _listService;
        private readonly IToDoService _todoService;

        public DeleteListScenario(IUserService userService, IToDoListService listService, IToDoService todoService)
        {
            _userService = userService;
            _listService = listService;
            _todoService = todoService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteList;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
        {
            var chatId = message.Chat.Id;
            var userId = message.From?.Id ?? 0;

            if (!context.Data.TryGetValue("User", out var userObj) || userObj is not ToDoUser user)
            {
                await bot.SendMessage(chatId, "Сначала зарегистрируйтесь через /start", cancellationToken: ct);
                return ScenarioResult.Completed;
            }

            switch (context.CurrentStep)
            {
                case null:
                    var lists = await _listService.GetUserLists(user.Id, ct);
                    if (!lists.Any())
                    {
                        await bot.SendMessage(chatId, "У вас нет списков для удаления.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    var buttons = lists.Select(list =>
                    {
                        var callbackData = $"deletelist_{list.Id}";
                        return InlineKeyboardButton.WithCallbackData($"❌ {list.Name}", callbackData);
                    }).ToList();

                    var keyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }));

                    await bot.SendMessage(chatId, "Выберите список для удаления:", replyMarkup: keyboard, cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    context.Data["Lists"] = lists;
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