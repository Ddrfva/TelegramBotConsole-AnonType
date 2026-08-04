using System;
using System.Threading.Tasks;
using Core.Services;
using Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot_28.TelegramBot.Scenarios
{
    public class AddListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _listService;

        public AddListScenario(IUserService userService, IToDoListService listService)
        {
            _userService = userService;
            _listService = listService;
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.AddList;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
        {
            var chatId = message.Chat.Id;
            var userId = message.From?.Id ?? 0;

            switch (context.CurrentStep)
            {
                case null:
                    var user = (ToDoUser)context.Data["User"];
                    context.CurrentStep = "Name";
                    await bot.SendMessage(chatId, "Введите название списка (не более 10 символов):", cancellationToken: ct);
                    return ScenarioResult.Transition;

                case "Name":
                    var listName = message.Text?.Trim();
                    if (string.IsNullOrWhiteSpace(listName) || listName.Length > 10)
                    {
                        await bot.SendMessage(chatId, "Название списка должно быть от 1 до 10 символов. Попробуйте снова:", cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                    try
                    {
                        var userObj = (ToDoUser)context.Data["User"];
                        var newList = await _listService.Add(userObj, listName, ct);
                        await bot.SendMessage(chatId, $"Список \"{newList.Name}\" создан.", cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await bot.SendMessage(chatId, $"Ошибка: {ex.Message}", cancellationToken: ct);
                    }
                    return ScenarioResult.Completed;

                default:
                    await bot.SendMessage(chatId, "Неизвестный шаг. Начните заново с /addlist.", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }
    }
}